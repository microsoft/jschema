﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Json.Schema.ToDotNet.Hints;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Generates a set of .NET classes from a JSON schema.
    /// </summary>
    public class DataModelGenerator
    {
        private readonly DataModelGeneratorSettings _settings;
        private readonly IFileSystem _fileSystem;
        private JsonSchema _rootSchema;
        private Dictionary<string, string> _pathToFileContentsDictionary;
        private List<string> _generatedClassNames;
        private string _kindEnumName;
        private string _nodeInterfaceName;
        private List<AdditionalTypeRequiredInfo> _additionalTypesRequiredList;
        private Dictionary<string, PropertyInfoDictionary> _classInfoDictionary;

        public DataModelGenerator(DataModelGeneratorSettings settings)
            : this(settings, new FileSystem())
        {
        }

        // This ctor allows unit tests to mock the file system.
        internal DataModelGenerator(DataModelGeneratorSettings settings, IFileSystem fileSystem)
        {
            _settings = settings;
            _settings.Validate();

            _fileSystem = fileSystem;
            _pathToFileContentsDictionary = new Dictionary<string, string>();

            _additionalTypesRequiredList = new List<AdditionalTypeRequiredInfo>();
            _generatedClassNames = new List<string>();

            _classInfoDictionary = new Dictionary<string, PropertyInfoDictionary>();
        }

        public string Generate(JsonSchema rootSchema)
        {
            _additionalTypesRequiredList.Clear();

            if (_settings.GenerateCloningCode)
            {
                _kindEnumName = _settings.SchemaName + "NodeKind" + _settings.TypeNameSuffix;
                _nodeInterfaceName = "I" + _settings.SchemaName + "Node" + _settings.TypeNameSuffix;
            }

            _rootSchema = JsonSchema.Collapse(rootSchema);

            if (_fileSystem.DirectoryExists(_settings.OutputDirectory) && !_settings.ForceOverwrite)
            {
                throw Error.CreateException(Resources.ErrorOutputDirectoryExists, _settings.OutputDirectory);
            }

            _fileSystem.CreateDirectory(_settings.OutputDirectory);

            SchemaType rootSchemaType = _rootSchema.SafeGetType();
            if (rootSchemaType != SchemaType.Object)
            {
                throw Error.CreateException(Resources.ErrorNotAnObject, rootSchemaType.ToString().ToLowerInvariant());
            }

            string rootFileText = GenerateClass(_settings.RootClassName, _rootSchema);

            if (_settings.GenerateEqualityComparers)
            {
                GenerateEqualityComparer(_settings.RootClassName, _rootSchema);
            }

            if (_rootSchema.Definitions != null)
            {
                List<KeyValuePair<string, JsonSchema>> typeDefinitions = _rootSchema.Definitions.Where(ShouldGenerateType).ToList();
                GenerateClassesForDefinitions(typeDefinitions);

                if (_settings.GenerateEqualityComparers)
                {
                    GenerateEqualityComparers(typeDefinitions);
                }
            }

            foreach (AdditionalTypeRequiredInfo additionalTypeRequiredInfo in _additionalTypesRequiredList)
            {
                GenerateAdditionalType(additionalTypeRequiredInfo.Hint, additionalTypeRequiredInfo.Schema);
            }

            if (_settings.GenerateCloningCode)
            {
                _pathToFileContentsDictionary[_nodeInterfaceName + _settings.TypeNameSuffix] =
                    GenerateSyntaxInterface(_settings.SchemaName, _kindEnumName, _nodeInterfaceName);

                _pathToFileContentsDictionary[_kindEnumName + _settings.TypeNameSuffix] =
                    GenerateKindEnum(_kindEnumName);

                string rewritingVisitorClassName = _settings.SchemaName + "RewritingVisitor" + _settings.TypeNameSuffix;
                _pathToFileContentsDictionary[rewritingVisitorClassName] =
                    new RewritingVisitorGenerator(
                        _classInfoDictionary,
                        _settings.CopyrightNotice,
                        _settings.SuffixedNamespaceName,
                        rewritingVisitorClassName,
                        _settings.SchemaName,
                        _kindEnumName,
                        _nodeInterfaceName,
                        _generatedClassNames)
                        .GenerateRewritingVisitor();
            }

            foreach (KeyValuePair<string, string> entry in _pathToFileContentsDictionary)
            {
                _fileSystem.WriteAllText(Path.Combine(_settings.OutputDirectory, entry.Key + ".cs"), entry.Value);
            }

            // Returning the text of the file generated from the root schema allows this method
            // to be more easily unit tested.
            return rootFileText;
        }

        private bool ShouldGenerateType(KeyValuePair<string, JsonSchema> definition)
        {
            return definition.Value.SafeGetType() != SchemaType.Array;
        }

        private void GenerateClassesForDefinitions(IEnumerable<KeyValuePair<string, JsonSchema>> definitions)
        {
            foreach (KeyValuePair<string, JsonSchema> definition in definitions)
            {
                GenerateClass(definition.Key, definition.Value);
            }
        }

        private void GenerateEqualityComparers(IEnumerable<KeyValuePair<string, JsonSchema>> definitions)
        {
            foreach (KeyValuePair<string, JsonSchema> definition in definitions)
            {
                GenerateEqualityComparer(definition.Key, definition.Value);
            }
        }

        private string GetHintedClassName(string className)
        {
            ClassNameHint classNameHint = _settings.HintDictionary?.GetHint<ClassNameHint>(className);
            if (classNameHint != null)
            {
                className = classNameHint.ClassName;
            }

            return className;
        }

        private void GenerateEqualityComparer(string className, JsonSchema schema)
        {
            className = GetHintedClassName(className).ToPascalCase() + _settings.TypeNameSuffix;

            var equalityComparerGenerator = new EqualityComparerGenerator(_settings.CopyrightNotice, _settings.SuffixedNamespaceName);

            string equalityComparerText = equalityComparerGenerator.Generate(
                className,
                _classInfoDictionary[className]);

            _pathToFileContentsDictionary[EqualityComparerGenerator.GetEqualityComparerClassName(className)]
                = equalityComparerText;
        }

        private string GenerateSyntaxInterface(string schemaName, string enumName, string syntaxInterfaceName)
        {
            PropertyDeclarationSyntax syntaxKindPropertyDeclaration =
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.ParseTypeName(enumName),
                    enumName)
                    .AddAccessorListAccessors(SyntaxHelper.MakeGetAccessor())
                    .WithLeadingTrivia(
                        SyntaxHelper.MakeDocComment(
                            string.Format(CultureInfo.CurrentCulture, Resources.SyntaxInterfaceKindDescription, syntaxInterfaceName)));

            MethodDeclarationSyntax deepCloneMethodDeclaration =
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName(_nodeInterfaceName),
                    "DeepClone")
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    .WithLeadingTrivia(
                        SyntaxHelper.MakeDocComment(Resources.SyntaxInterfaceDeepCloneDescription));

            InterfaceDeclarationSyntax interfaceDeclaration =
                SyntaxFactory.InterfaceDeclaration(_nodeInterfaceName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddMembers(
                        syntaxKindPropertyDeclaration,
                        deepCloneMethodDeclaration);

            string summaryComment = string.Format(
                CultureInfo.CurrentCulture,
                Resources.SyntaxInterfaceDescription,
                schemaName);

            return interfaceDeclaration.Format(
                _settings.CopyrightNotice,
                null, // usings
                _settings.SuffixedNamespaceName,
                summaryComment);
        }

        private string GenerateKindEnum(string enumName)
        {
            EnumDeclarationSyntax enumDeclaration =
                SyntaxFactory.EnumDeclaration(SyntaxFactory.Identifier(enumName))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddMembers(
                        SyntaxFactory.EnumMemberDeclaration("None")
                            .WithLeadingTrivia(
                                SyntaxHelper.MakeDocComment(Resources.KindEnumNoneDescription)))
                    .AddMembers(
                        _generatedClassNames.Select(gcn => GenerateKindEnumMember(gcn)).ToArray());

            string summaryComment = string.Format(
                CultureInfo.CurrentCulture,
                Resources.KindEnumDescription,
                _nodeInterfaceName);

            return enumDeclaration.Format(
                _settings.CopyrightNotice,
                null, // usings
                _settings.SuffixedNamespaceName,
                summaryComment);
        }

        private EnumMemberDeclarationSyntax GenerateKindEnumMember(string className)
        {
            string description = string.Format(
                CultureInfo.CurrentCulture,
                Resources.KindEnumMemberDescription,
                _nodeInterfaceName,
                className);

            return SyntaxFactory.EnumMemberDeclaration(className)
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(description));
        }

        internal string GenerateClass(
            string className,
            JsonSchema schema)
        {
            className = GetHintedClassName(className).ToPascalCase();
            string suffixedClassName = className + _settings.TypeNameSuffix;

            var propertyInfoDictionary = new PropertyInfoDictionary(
                className,
                _settings.TypeNameSuffix,
                schema,
                _settings.HintDictionary,
                OnAdditionalTypeRequired);

            _classInfoDictionary.Add(suffixedClassName, propertyInfoDictionary);

            EnumHint enumHint = null;
            InterfaceHint interfaceHint = null;
            if (_settings.HintDictionary != null)
            {
                string key = className.ToCamelCase();
                enumHint = _settings.HintDictionary.GetHint<EnumHint>(key);
                interfaceHint = _settings.HintDictionary.GetHint<InterfaceHint>(key);
            }

            string baseInterfaceName = null;
            if (interfaceHint != null)
            {
                baseInterfaceName = "I" + suffixedClassName;
            }

            TypeGenerator typeGenerator;
            if (enumHint == null)
            {
                typeGenerator = new ClassGenerator(
                    propertyInfoDictionary,
                    schema,
                    _settings.HintDictionary,
                    baseInterfaceName,
                    _settings.GenerateEqualityComparers,
                    _settings.GenerateCloningCode,
                    _settings.SealClasses,
                    _settings.VirtualMembers,
                    _nodeInterfaceName,
                    _kindEnumName,
                    _settings.TypeNameSuffix);

                if (_settings.GenerateCloningCode)
                {
                    // The cloning code includes an enumeration with one member for each
                    // generated class, so keep track of the class names.
                    _generatedClassNames.Add(suffixedClassName);
                }
            }
            else
            {
                typeGenerator = new EnumGenerator(schema, _settings.TypeNameSuffix, _settings.HintDictionary);
            }
        
            _pathToFileContentsDictionary[suffixedClassName] = typeGenerator.Generate(
                _settings.SuffixedNamespaceName,
                className,
                _settings.CopyrightNotice,
                schema.Description);

            if (interfaceHint != null)
            {
                typeGenerator = new InterfaceGenerator(
                    propertyInfoDictionary,
                    schema,
                    _settings.TypeNameSuffix,
                    _settings.HintDictionary);
                string description = interfaceHint.Description ?? schema.Description;

                _pathToFileContentsDictionary[baseInterfaceName + _settings.TypeNameSuffix] = typeGenerator.Generate(
                    _settings.SuffixedNamespaceName,
                    baseInterfaceName,
                    _settings.CopyrightNotice,
                    description);
            }

            return _pathToFileContentsDictionary[suffixedClassName];
        }

        private void OnAdditionalTypeRequired(AdditionalTypeRequiredInfo e)
        {
            _additionalTypesRequiredList.Add(e);
        }

        private void GenerateAdditionalType(CodeGenHint hint, JsonSchema schema)
        {
            // We do not handle the case where generating an additional type
            // causes still get another type to be generated. It wouldn't be hard
            // to add if needed.
            var enumHint = hint as EnumHint;
            if (enumHint != null)
            {
                GenerateAdditionalTypeFromEnumHint(enumHint, schema);
            }
            else
            {
                throw Error.CreateException(
                                Resources.ErrorCannotGenerateAdditionalTypeFromHintType,
                                nameof(CodeGenHint),
                                hint.GetType().Name);
            }
        }

        private void GenerateAdditionalTypeFromEnumHint(EnumHint enumHint, JsonSchema schema)
        {
            if (enumHint.AllowMemberCountMismatch == false
                && enumHint.MemberNames != null
                && schema.Enum != null
                && enumHint.MemberNames.Length != schema.Enum.Count)
            {
                throw Error.CreateException(
                                Resources.ErrorMismatchedEnumCount,
                                nameof(EnumHint),
                                enumHint.TypeName,
                                enumHint.MemberNames.Length,
                                schema.Enum.Count);
            }

            var enumNames = new List<string>();
            if (!string.IsNullOrWhiteSpace(enumHint.ZeroValueName))
            {
                enumNames.Add(enumHint.ZeroValueName);
            }

            if (enumHint.MemberNames != null)
            {
                enumNames.AddRange(enumHint.MemberNames);
            }
            else
            {
                enumNames.AddRange(schema.Enum.Select(e => e.ToString()));
            }

            var enumTypeSchema = new JsonSchema
            {
                Description = enumHint.Description ?? schema.Description,
                Enum = enumNames.Cast<object>().ToList()
            };

            var generator = new EnumGenerator(enumTypeSchema, _settings.TypeNameSuffix, _settings.HintDictionary);
            _pathToFileContentsDictionary[enumHint.TypeName + _settings.TypeNameSuffix] =
                generator.Generate(
                    _settings.SuffixedNamespaceName,
                    enumHint.TypeName,
                    _settings.CopyrightNotice,
                    enumTypeSchema.Description);
        }
    }
}
