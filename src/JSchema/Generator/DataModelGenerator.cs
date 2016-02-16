// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace MountBaker.JSchema.Generator
{
    /// <summary>
    /// Generates a set of .NET classes from a JSON schema.
    /// </summary>
    public class DataModelGenerator
    {
        private readonly DataModelGeneratorSettings _settings;
        private readonly IFileSystem _fileSystem;
        private readonly AdhocWorkspace _workspace;

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
            _workspace = new AdhocWorkspace();
        }

        public void Generate(JsonSchema schema)
        {
            if (_fileSystem.DirectoryExists(_settings.OutputDirectory) && !_settings.ForceOverwrite)
            {
                throw JSchemaException.Create(Resources.ErrorOutputDirectoryExists, _settings.OutputDirectory);
            }

            _fileSystem.CreateDirectory(_settings.OutputDirectory);

            if (schema.Type != JsonType.Object)
            {
                throw JSchemaException.Create(Resources.ErrorNotAnObject, schema.Type);
            }

            CreateFile(_settings.RootClassName, schema);
        }

        internal void CreateFile(string className, JsonSchema schema)
        {
            string text = CreateFileText(className, schema);
            _fileSystem.WriteAllText(Path.Combine(_settings.OutputDirectory, className + ".cs"), text);
        }

        private string CreateFileText(string className, JsonSchema schema)
        {
            // Hat tip: Mike Bennett, "Generating Code with Roslyn",
            // https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
            CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit();

            NamespaceDeclarationSyntax ns =
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_settings.NamespaceName));

            ClassDeclarationSyntax cls = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            if (schema.Properties != null)
            {
                var props = new List<PropertyDeclarationSyntax>();
                foreach (KeyValuePair<string, JsonSchema> schemaProperty in schema.Properties)
                {
                    string propertyName = schemaProperty.Key;
                    JsonSchema subSchema = schemaProperty.Value;

                    if (subSchema.Type == JsonType.Object)
                    {
                        CreateFile(propertyName, subSchema);
                    }

                    var modifiers = new SyntaxTokenList()
                        .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                    SyntaxKind typeKeyword = GetTypeKeywordFromJsonType(subSchema.Type);

                    var accessorDeclarations = new SyntaxList<AccessorDeclarationSyntax>()
                        .AddRange(new AccessorDeclarationSyntax[]
                        {
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        });

                    PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(
                        default(SyntaxList<AttributeListSyntax>),
                        modifiers,
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword)),
                        default(ExplicitInterfaceSpecifierSyntax),
                        SyntaxFactory.Identifier(Capitalize(propertyName)),
                        SyntaxFactory.AccessorList(accessorDeclarations));

                    props.Add(prop);
                }

                cls = cls.AddMembers(props.ToArray());
            }

            ns = ns.AddMembers(cls);
            cu = cu.AddMembers(ns);

            SyntaxNode formattedNode = Formatter.Format(cu, _workspace);
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            return sb.ToString();
        }

        private static string Capitalize(string propertyName)
        {
            return propertyName[0].ToString().ToUpperInvariant() + propertyName.Substring(1);
        }

        private static readonly Dictionary<JsonType, SyntaxKind> s_jsonTypeToSyntaxKindDictionary = new Dictionary<JsonType, SyntaxKind>
        {
            [JsonType.Boolean] = SyntaxKind.BoolKeyword,
            [JsonType.Integer] = SyntaxKind.IntKeyword,
            [JsonType.Number] = SyntaxKind.DoubleKeyword,
            [JsonType.String] = SyntaxKind.StringKeyword
        };

        private static SyntaxKind GetTypeKeywordFromJsonType(JsonType type)
        {
            SyntaxKind typeKeyword;
            if (!s_jsonTypeToSyntaxKindDictionary.TryGetValue(type, out typeKeyword))
            {
                typeKeyword = SyntaxKind.ObjectKeyword;
            }

            return typeKeyword;
        }
    }
}
