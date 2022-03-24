// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Json.Schema.ToDotNet.Hints;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Generate the text of a class.
    /// </summary>
    public class ClassGenerator : ClassOrInterfaceGenerator
    {
        private readonly string _baseInterfaceName;
        private readonly bool _generateEqualityComparers;
        private readonly bool _generateComparers;
        private readonly bool _generateCloningCode;
        private readonly bool _sealClasses;
        private readonly bool _virtualMembers;
        private readonly bool _protectedInitMethods;
        private readonly string _syntaxInterfaceName;
        private readonly string _kindEnumName;

        // Name used for the parameters of the copy ctor.
        private const string OtherParameterName = "other";

        private const string DefaultValueAttributeName = "DefaultValue";
        private const string DefaultValueAttributeNamespaceName = "System.ComponentModel";
        private const string GenericCollectionsNamespaceName = "System.Collections.Generic";

        private const string JsonPropertyAttributeName = "JsonProperty";
        private const string JsonPropertyAttributeNamespaceName = "Newtonsoft.Json";
        private const string DefaultValueHandlingValueName = "IgnoreAndPopulate";
        private const string DefaultValueHandlingPropertyName = "DefaultValueHandling";
        private const string DefaultValueHandlingEnumerationName = "DefaultValueHandling";

        private const string DataContractAttributeName = "DataContract";
        private const string DataMemberNamePropertyName = "Name";
        private const string DataMemberAttributeName = "DataMember";
        private const string DataMemberIsRequiredPropertyName = "IsRequired";
        private const string DataMemberEmitDefaultValuePropertyName = "EmitDefaultValue";

        private const string AddMethodName = "Add";
        private const string InitMethodName = "Init";
        private const string DeepCloneMethodName = "DeepClone";
        private const string DeepCloneCoreMethodName = "DeepCloneCore";

        private const string ValueEqualsMethodName = "ValueEquals";
        private const string ValueGetHashCodeMethodName = "ValueGetHashCode";

        private const string KeyPropertyName = "Key";
        private const string ValuePropertyName = "Value";
        private const string ComparerPropertyName = "Comparer";
        private const string ValueComparerPropertyName = "ValueComparer";

        private LocalVariableNameGenerator _localVariableNameGenerator;

        public ClassGenerator(
            PropertyInfoDictionary propertyInfoDictionary,
            JsonSchema schema,
            HintDictionary hintDictionary,
            string interfaceName,
            bool generateEqualityComparers,
            bool generateComparers,
            bool generateCloningCode,
            bool sealClasses,
            bool virtualMembers,
            bool protectedInitMethods,
            string syntaxInterfaceName,
            string kindEnumName,
            string typeNameSuffix)
            : base(propertyInfoDictionary, schema, typeNameSuffix, hintDictionary)
        {
            _baseInterfaceName = interfaceName;
            _generateEqualityComparers = generateEqualityComparers;
            _generateComparers = generateComparers;
            _generateCloningCode = generateCloningCode;
            _syntaxInterfaceName = syntaxInterfaceName;
            _sealClasses = sealClasses;
            _virtualMembers = virtualMembers;
            _protectedInitMethods = protectedInitMethods;
            _kindEnumName = kindEnumName;

            _localVariableNameGenerator = new LocalVariableNameGenerator();
        }

        public override BaseTypeDeclarationSyntax GenerateTypeDeclaration()
        {
            SyntaxKind sealedOrPartial = _sealClasses
                ? SyntaxKind.SealedKeyword
                : SyntaxKind.PartialKeyword;

            var classDeclaration = SyntaxFactory.ClassDeclaration(SuffixedTypeName)
                .AddAttributeLists(new AttributeListSyntax[]
                    {
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName(DataContractAttributeName))))
                    })
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(sealedOrPartial));

            var baseTypes = new List<BaseTypeSyntax>();

            BaseTypeHint baseTypeHint = HintDictionary?.GetHint<BaseTypeHint>(TypeName.ToCamelCase());
            if (baseTypeHint?.BaseTypeNames != null)
            {
                baseTypes.AddRange(
                    baseTypeHint.BaseTypeNames.Select(
                        btn => SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.ParseTypeName(btn))));
            }

            // If this class implements an interface, add the interface to
            // the base type list.
            if (_baseInterfaceName != null)
            {
                SimpleBaseTypeSyntax interfaceType =
                    SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.ParseTypeName(_baseInterfaceName));

                baseTypes.Add(interfaceType);
            }

            // If we were asked to generate cloning code, add the necessary interface.
            if (_generateCloningCode)
            {
                SimpleBaseTypeSyntax interfaceType =
                    SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.ParseTypeName(_syntaxInterfaceName));

                baseTypes.Add(interfaceType);
            }

            AddUsing("System");
            AddUsing("System.Runtime.Serialization");   // For DataContractAttribute;

            if (baseTypes.Count > 0)
            {
                SeparatedSyntaxList<BaseTypeSyntax> separatedBaseList = SyntaxFactory.SeparatedList(baseTypes);
                BaseListSyntax baseList = SyntaxFactory.BaseList(separatedBaseList);
                classDeclaration = classDeclaration.WithBaseList(baseList);
            }

            return classDeclaration;
        }

        public override void AddMembers()
        {
            var members = new List<MemberDeclarationSyntax>();

            if (_generateEqualityComparers)
            {
                // For IEqualityComparer<T>.
                Usings.Add(GenericCollectionsNamespaceName);

                members.Add(GenerateValueComparerProperty());
                members.Add(GenerateValueEqualsMethod());
                members.Add(GenerateValueGetHashCodeMethod());
            }

            if (_generateComparers)
            {
                Usings.Add(GenericCollectionsNamespaceName);
                members.Add(GenerateComparerProperty());
            }

            if (_generateCloningCode)
            {
                members.Add(GenerateSyntaxKindProperty());
            }
                
            members.AddRange(GenerateProperties());

            if (_generateCloningCode)
            {
                members.AddRange(new MemberDeclarationSyntax[]
                {
                    GenerateDefaultConstructor(),
                    GeneratePropertyCtor(),
                    GenerateCopyConstructor(),
                    GenerateISyntaxDeepClone(),
                    GenerateDeepClone(),
                    GenerateDeepCloneCore(),
                    GenerateInitMethod()
                });
            }

            TypeDeclaration = (TypeDeclaration as ClassDeclarationSyntax).AddMembers(members.ToArray());
        }

        private MemberDeclarationSyntax GenerateValueComparerProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(
                    EqualityComparerGenerator.GetComparerBaseType(SuffixedTypeName).ToString()),
                ValueComparerPropertyName)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(
                                EqualityComparerGenerator.GetEqualityComparerClassName(SuffixedTypeName)),
                            SyntaxFactory.IdentifierName(
                                EqualityComparerGenerator.InstancePropertyName))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private MemberDeclarationSyntax GenerateComparerProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(
                    ComparerCodeGenerator.GetComparerBaseType(SuffixedTypeName).ToString()),
                ComparerPropertyName)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(
                                ComparerCodeGenerator.GetComparerClassName(SuffixedTypeName)),
                            SyntaxFactory.IdentifierName(
                                EqualityComparerGenerator.InstancePropertyName))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private MemberDeclarationSyntax GenerateValueEqualsMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(ValueEqualsMethodName))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(OtherParameterName))
                                .WithType(SyntaxFactory.ParseTypeName(SuffixedTypeName)))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(ValueComparerPropertyName),
                                SyntaxFactory.IdentifierName(WellKnownMethodNames.EqualsMethod)),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.ThisExpression(),
                                SyntaxFactory.IdentifierName(OtherParameterName)))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private MemberDeclarationSyntax GenerateValueGetHashCodeMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                SyntaxFactory.Identifier(ValueGetHashCodeMethodName))
                .WithParameterList(
                    SyntaxFactory.ParameterList())
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(ValueComparerPropertyName),
                                SyntaxFactory.IdentifierName(WellKnownMethodNames.GetHashCodeMethod)),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.ThisExpression()))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        protected override AttributeSyntax[] GeneratePropertyAttributes(string propertyName, string serializedName, bool isRequired, object defaultValue, TypeSyntax propertyType)
        {
            var attributes = new List<AttributeSyntax>();

            var dataMemberAttributeArguments =
                new List<AttributeArgumentSyntax>
                {
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(DataMemberNamePropertyName),
                        default(NameColonSyntax),
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(serializedName))),
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(DataMemberIsRequiredPropertyName),
                        default(NameColonSyntax),
                        SyntaxFactory.LiteralExpression(isRequired
                            ? SyntaxKind.TrueLiteralExpression
                            : SyntaxKind.FalseLiteralExpression))
                };

            if (!isRequired)
            {
                dataMemberAttributeArguments.Add(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(DataMemberEmitDefaultValuePropertyName),
                        default(NameColonSyntax),
                        SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));
            }

            AttributeSyntax dataMemberAttribute =
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName(DataMemberAttributeName),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(dataMemberAttributeArguments)));

            attributes.Add(dataMemberAttribute);

            if (defaultValue != null)
            {
                // We want to add a DefaultValue attribute, but we can only do that if the
                // default value specified in the schema can be represented by a literal
                // (a compile-time constant like 42 or "Don't panic" or Color.Red). We can't do it if
                // the default value must be calculated at runtime, for example, an empty array.
                ExpressionSyntax expression = GetExpressionForValue(defaultValue, propertyType);
                if (expression != null)
                {
                    AddUsing(DefaultValueAttributeNamespaceName);

                    var defaultValueArguments = new List<AttributeArgumentSyntax>
                    {
                        SyntaxFactory.AttributeArgument(expression)
                    };

                    AttributeSyntax defaultValueAttribute =
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName(DefaultValueAttributeName),
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SeparatedList(defaultValueArguments)));

                    attributes.Add(defaultValueAttribute);
                }

                AddUsing(JsonPropertyAttributeNamespaceName);

                var jsonPropertyArguments = new List<AttributeArgumentSyntax>
                {
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(DefaultValueHandlingPropertyName),
                        default(NameColonSyntax),
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(DefaultValueHandlingEnumerationName),
                            SyntaxFactory.IdentifierName(DefaultValueHandlingValueName)))
                };

                AttributeSyntax jsonPropertyAttribute =
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName(JsonPropertyAttributeName),
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SeparatedList(jsonPropertyArguments)));

                attributes.Add(jsonPropertyAttribute);
            }

            string hintDictionaryKey = MakeHintDictionaryKey(propertyName);
            AttributeHint[] attributeHints = HintDictionary?.GetHints<AttributeHint>(hintDictionaryKey);
            if (attributeHints != null)
            {
                foreach (AttributeHint attributeHint in attributeHints)
                {
                    AttributeSyntax hintedAttribute =
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName(attributeHint.TypeName));

                    if (attributeHint.NamespaceName != null)
                    {
                        AddUsing(attributeHint.NamespaceName);
                    }

                    if (attributeHint.Arguments?.Count > 0)
                    {
                        hintedAttribute = hintedAttribute
                            .AddArgumentListArguments(
                                attributeHint.Arguments.Select(
                                    arg => SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(arg))).ToArray());
                    }

                    if (attributeHint.Properties?.Count > 0)
                    {
                        hintedAttribute = hintedAttribute
                            .AddArgumentListArguments(
                                attributeHint.Properties.Select(
                                    prop => SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.NameEquals(prop.Key),
                                        default(NameColonSyntax),
                                        SyntaxFactory.ParseExpression(prop.Value))).ToArray());
                    }

                    attributes.Add(hintedAttribute);
                }
            }

            return attributes.ToArray();
        }

        /// <summary>
        /// Generates the modifiers for a property specified in the schema.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property whose modifiers are to be generated.
        /// </param>
        /// <returns>
        /// An array of <code>SyntaxToken</code> describing the modifiers.
        /// </returns>
        protected override SyntaxToken[] GenerateSchemaPropertyModifiers(string propertyName)
        {
            PropertyModifiersHint propertyModifiersHint = HintDictionary?.GetPropertyHint<PropertyModifiersHint>(TypeName, propertyName);

            IList<SyntaxToken> modifierTokens;
            if (propertyModifiersHint?.Modifiers != null)
            {
                modifierTokens = propertyModifiersHint.Modifiers;
            }
            else
            {
                modifierTokens = new List<SyntaxToken>
                {
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                };

                if (_virtualMembers)
                {
                    modifierTokens.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
                }
            }

            return modifierTokens.ToArray();
        }

        protected override AccessorDeclarationSyntax[] GeneratePropertyAccessors()
        {
            return new AccessorDeclarationSyntax[]
                {
                    SyntaxHelper.MakeGetAccessor(),
                    SyntaxHelper.MakeSetAccessor()
                };
        }

        private PropertyDeclarationSyntax GenerateSyntaxKindProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(_kindEnumName),
                _kindEnumName)
                .AddModifiers(GenerateNonSchemaPropertyModifiers(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                        SyntaxHelper.MakeGetAccessor(
                            SyntaxFactory.Block(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(_kindEnumName),
                                        SyntaxFactory.IdentifierName(SuffixedTypeName))))))
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(CultureInfo.CurrentCulture, Resources.SyntaxInterfaceKindDescription, _syntaxInterfaceName)));
        }

        private ConstructorDeclarationSyntax GenerateDefaultConstructor()
        {
            return SyntaxFactory.ConstructorDeclaration(SuffixedTypeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBodyStatements(GenerateDefaultInitializations())
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.DefaultCtorSummary,
                            SuffixedTypeName)));
        }

        /// <summary>
        /// Generates an initialization statement for each property for which the
        /// schema specifies a default value.
        /// </summary>
        /// <remarks>
        /// The resulting statements are inserted into the default constructor.
        /// This ensures that the default values are set even if the object is
        /// not the result of deserializing a JSON instance document.
        /// <remarks>
        /// <returns>
        /// An array containing one initialization statement for each property
        /// for which the schema specifies a default value.
        /// </returns>
        private ExpressionStatementSyntax[] GenerateDefaultInitializations()
        {
            var initializations = new List<ExpressionStatementSyntax>();

            foreach (string propertyName in PropInfoDictionary.GetPropertyNames())
            {
                if (IncludeProperty(propertyName))
                {
                    PropertyInfo propInfo = PropInfoDictionary[propertyName];
                    object defaultValue = propInfo.DefaultValue;

                    ExpressionStatementSyntax initializationStatement = GenerateDefaultInitialization(propertyName, defaultValue, propInfo.Type);
                    if (initializationStatement != null)
                    {
                        initializations.Add(initializationStatement);
                    }
                }
            }

            return initializations.ToArray();
        }

        private ExpressionStatementSyntax GenerateDefaultInitialization(
            string propertyName,
            object defaultValue,
            TypeSyntax propertyType)
        {
            ExpressionSyntax defaultValueExpression = GetExpressionForValue(defaultValue, propertyType);
            if (defaultValueExpression == null) { return null; }

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    defaultValueExpression));
        }

        private ExpressionSyntax GetExpressionForValue(object value, TypeSyntax propertyType)
        {
            ExpressionSyntax expression = null;

            if (value is bool boolValue)
            {
                SyntaxKind literalSyntaxKind = boolValue
                    ? SyntaxKind.TrueLiteralExpression
                    : SyntaxKind.FalseLiteralExpression;

                expression = SyntaxFactory.LiteralExpression(literalSyntaxKind);
            }
            else if (value is long longValue)
            {
                expression = SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal((int)longValue));
                // Note: the extra cast compensates for a mismatch between our code generation
                // and Newtonsoft.Json's deserialization behavior. Newtonsoft deserializes
                // integer properties as Int64 (long), but we generate integer properties
                // with type int. The extra cast causes Roslyn to emit the literal 42,
                // which can be assigned to an int, rather than 42L, which cannot. We should
                // consider changing the code generation to emit longs for integer properties.
            }
            else if (value is double doubleValue)
            {
                expression = SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(doubleValue));
            }
            else if (value is string stringValue)
            {
                // When the schema specifies a string value for the default, it is either
                // because the property is a plain string, or because it is an enumerated
                // type. If it's an enumerated type, like Color, then we have to generate
                // an enumeration constant like Color.Green, and not a string literal like
                // "green".
                if (propertyType is PredefinedTypeSyntax)
                {
                    expression = SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(stringValue));
                }
                else if (propertyType is IdentifierNameSyntax identifierNameSyntax)
                {
                    string enumerationConstantName = stringValue.ToPascalCase();

                    expression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(identifierNameSyntax.Identifier.ValueText),
                        SyntaxFactory.IdentifierName(enumerationConstantName));
                }
            }

            return expression;
        }

        private ConstructorDeclarationSyntax GeneratePropertyCtor()
        {
            // Generate the argument list that will be passed from the copy ctor to the
            // Init method.
            ExpressionSyntax[] arguments = PropInfoDictionary.GetPropertyNames()
                .Select(name =>  SyntaxFactory.IdentifierName(name.ToCamelCase()))
                .ToArray();

            return SyntaxFactory.ConstructorDeclaration(SuffixedTypeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    // This ctor takes the same parameters as the Init method, so use the
                    // same helper method to generate the parameter list.
                    GenerateInitMethodParameterList())
                .AddBodyStatements(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(InitMethodName),
                            SyntaxHelper.ArgumentList(arguments))))
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.PropertyCtorSummary,
                            SuffixedTypeName),
                        paramDescriptionDictionary: MakePropertyCtorParamDescriptions()));
        }

        /// <summary>
        /// Synthesize the type of the property as it should appear in the parameter list
        /// of the generated class's <code>Init</code> method.
        /// </summary>
        /// <remarks>
        /// For array-valued properties, the property type stored in the
        /// PropertyInfoDictionary is <see cref="IList{T}" />. But in the parameter list
        /// of the <code>Init</code> method, the type appears as
        /// <see cref="IEnumerable{T}" />.
        /// </remarks>
        private TypeSyntax GetParameterListType(string name)
        {
            TypeSyntax type = PropInfoDictionary[name].Type;

            if (PropInfoDictionary[name].ComparisonKind == ComparisonKind.Collection)
            {
                string typeName = type.ToString().Replace("IList<", "IEnumerable<");
                type = SyntaxFactory.ParseTypeName(typeName);
            }

            return type;
        }

        private Dictionary<string, string> MakePropertyCtorParamDescriptions()
        {
            var result = new Dictionary<string, string>();

            foreach (string propertyName in PropInfoDictionary.GetPropertyNames())
            {
                string paramName = propertyName.ToCamelCase();

                result[paramName] = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.PropertyCtorParamDescription,
                    propertyName);
            }

            return result;
        }

        private ConstructorDeclarationSyntax GenerateCopyConstructor()
        {
            // Generate the argument list that will be passed from the copy ctor to the
            // Init method.
            ExpressionSyntax[] initArguments = PropInfoDictionary.GetPropertyNames()
                .Select(name => 
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(OtherParameterName),
                        SyntaxFactory.IdentifierName(name)))
                    .ToArray();

            return SyntaxFactory.ConstructorDeclaration(SuffixedTypeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(OtherParameterName))
                            .WithType(SyntaxFactory.ParseTypeName(SuffixedTypeName)))
                .AddBodyStatements(
                    SyntaxFactory.IfStatement(
                        SyntaxHelper.IsNull(OtherParameterName),
                        SyntaxFactory.Block(
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.ParseTypeName("ArgumentNullException"))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.IdentifierName("nameof"),
                                                SyntaxHelper.ArgumentList(
                                                    SyntaxFactory.IdentifierName(OtherParameterName)))))))),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(InitMethodName),
                            SyntaxHelper.ArgumentList(initArguments))))
            .WithLeadingTrivia(
                SyntaxHelper.MakeDocComment(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.CopyCtorSummary,
                        SuffixedTypeName),
                    returns: null,
                    paramDescriptionDictionary: new Dictionary<string, string>
                    {
                        [OtherParameterName] = Resources.CopyCtorOtherParam
                    },
                    exceptionDictionary: new Dictionary<string, string>
                    {
                        ["ArgumentNullException"] = 
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.CopyCtorOtherNullException,
                                OtherParameterName)
                    }));
        }

        private MethodDeclarationSyntax GenerateISyntaxDeepClone()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(_syntaxInterfaceName),
                DeepCloneMethodName)
                .WithExplicitInterfaceSpecifier(
                    SyntaxFactory.ExplicitInterfaceSpecifier(
                        SyntaxFactory.IdentifierName(_syntaxInterfaceName)))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(DeepCloneCoreMethodName),
                            SyntaxHelper.ArgumentList())));
        }

        private MethodDeclarationSyntax GenerateDeepClone()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(SuffixedTypeName),
                DeepCloneMethodName)
                .AddModifiers(GenerateNonSchemaPropertyModifiers(SyntaxKind.PublicKeyword))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.ParseTypeName(SuffixedTypeName),
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName(DeepCloneCoreMethodName),
                                SyntaxHelper.ArgumentList()))))
                .WithLeadingTrivia(SyntaxHelper.MakeDocComment(Resources.DeepCloneDescription));
        }

        private MethodDeclarationSyntax GenerateDeepCloneCore()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(_syntaxInterfaceName),
                DeepCloneCoreMethodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.ParseTypeName(SuffixedTypeName),
                            SyntaxHelper.ArgumentList(SyntaxFactory.ThisExpression()),
                            default(InitializerExpressionSyntax))));
        }

        private MethodDeclarationSyntax GenerateInitMethod()
        {
            _localVariableNameGenerator.Reset();

            SyntaxKind[] modifierKinds = _protectedInitMethods ?
                new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.VirtualKeyword } :
                new[] { SyntaxKind.PrivateKeyword };

            SyntaxToken[] modifierTokens = modifierKinds
                .Select(mk => SyntaxFactory.Token(mk))
                .ToArray();

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                InitMethodName)
                .AddModifiers(modifierTokens)
                .AddParameterListParameters(GenerateInitMethodParameterList())
                .AddBodyStatements(GenerateInitializations());
        }

        private ParameterSyntax[] GenerateInitMethodParameterList()
        {
            return PropInfoDictionary.GetPropertyNames()
                .Select(name => SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(name.ToCamelCase()))
                    .WithType(GetParameterListType(name)))
                .ToArray();
        }

        private StatementSyntax[] GenerateInitializations()
        {
            var statements = new List<StatementSyntax>();

            foreach (string propertyName in PropInfoDictionary.GetPropertyNames())
            {
                StatementSyntax statement = GenerateInitialization(propertyName);
                if (statement != null)
                {
                    statements.Add(statement);
                }
            }

            return statements.ToArray();
        }

        private StatementSyntax GenerateInitialization(string propertyName)
        {
            InitializationKind initializationKind = PropInfoDictionary[propertyName].InitializationKind;

            switch (initializationKind)
            {
                case InitializationKind.SimpleAssign:
                    return GenerateSimpleAssignmentInitialization(propertyName);

                case InitializationKind.Clone:
                    return GenerateCloneInitialization(propertyName);

                case InitializationKind.Collection:
                    return GenerateCollectionInitialization(propertyName);

                case InitializationKind.Uri:
                    return GenerateUriInitialization(propertyName);

                case InitializationKind.Dictionary:
                    return GenerateDictionaryInitialization(propertyName);

                default:
                    // Do not generate initialization code for this property.
                    return null;
            }
        }

        private StatementSyntax GenerateSimpleAssignmentInitialization(string propertyName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.IdentifierName(propertyName.ToCamelCase())));
        }

        private StatementSyntax GenerateCloneInitialization(string propertyName)
        {
            // The name of the argument to the Init method is related to the name of the
            // property it will be used to initialize.
            string argName = propertyName.ToCamelCase();

            // Get the type of the concrete dictionary with which to initialize the
            // property. For example, if the property is of type IDictionary<string, double>,
            // it will be initialized with an object of type Dictionary<string, double>.
            TypeSyntax type = PropInfoDictionary.GetConcreteDictionaryType(propertyName);

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNotNull(argName),
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(propertyName),
                            SyntaxFactory.ObjectCreationExpression(
                                type,
                                SyntaxHelper.ArgumentList(
                                    SyntaxFactory.IdentifierName(argName)),
                                default(InitializerExpressionSyntax))))));
        }

        private StatementSyntax GenerateCollectionInitialization(string propertyName)
        {
            // The name of the argument to the Init method is related to the name of the
            // property it will be used to initialize.
            string argName = propertyName.ToCamelCase();

            // Get the type of the concrete collection with which to initialize the property.
            // For example, if the property is of type IList<int>, it will be initialized
            // with an object of type List<int>.
            TypeSyntax type = PropInfoDictionary.GetConcreteListType(propertyName);

            // The name of a temporary variable in which the collection values will be
            // accumulated.
            string destinationVariableName = _localVariableNameGenerator.GetNextDestinationVariableName();

            // The name of a variable used to loop over the elements of the argument
            // to the Init method (the argument whose name is "argName").
            string collectionElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();

            // Find out out kind of code must be generated to initialize the elements of
            // the collection.
            string elementInfoKey = PropertyInfoDictionary.MakeElementKeyName(propertyName);

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNotNull(SyntaxFactory.IdentifierName(argName)),
                SyntaxFactory.Block(
                    // var destination_0 = new List<D>();
                    DeclareCollection(type, destinationVariableName),

                    // foreach (var value_0 in foo)
                    GenerateElementInitializationLoop(
                        collectionElementVariableName,
                        SyntaxFactory.IdentifierName(argName),
                        elementInfoKey,
                        destinationVariableName),

                    // Foo = foo;
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(propertyName),
                            SyntaxFactory.IdentifierName(destinationVariableName)))));
        }

        private SyntaxToken[] GenerateNonSchemaPropertyModifiers(params SyntaxKind[] modifierKinds)
        {
            List<SyntaxKind> modifierKindList = modifierKinds.ToList();
            if (_virtualMembers)
            {
                modifierKindList.Add(SyntaxKind.VirtualKeyword);
            }

            return modifierKindList
                .Select(mk => SyntaxFactory.Token(mk))
                .ToArray();
        }

        private LocalDeclarationStatementSyntax DeclareCollection(
            TypeSyntax collectionType,
            string collectionVariableName)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxHelper.Var(),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(collectionVariableName),
                            default(BracketedArgumentListSyntax),
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(
                                    collectionType,
                                    SyntaxHelper.ArgumentList(),
                                    default(InitializerExpressionSyntax)))))));
        }

        private ForEachStatementSyntax GenerateElementInitializationLoop(
            string collectionElementVariableName,
            ExpressionSyntax collection,
            string elementInfoKey,
            string destinationVariableName)
        {
            return SyntaxFactory.ForEachStatement(
                SyntaxHelper.Var(),
                collectionElementVariableName,
                collection,
                SyntaxFactory.Block(
                    GenerateElementInitialization(
                        elementInfoKey,
                        destinationVariableName,
                        collectionElementVariableName)));
        }

        private StatementSyntax GenerateDictionaryInitialization(string propertyName)
        {
            string elementPropertyInfoKey = PropertyInfoDictionary.MakeDictionaryItemKeyName(propertyName);
            PropertyInfo elementInfo = PropInfoDictionary[elementPropertyInfoKey];

            switch (elementInfo.InitializationKind)
            {
                // If the elements can be copied, the dictionary itself can be cloned
                // (copy-constructed).
                case InitializationKind.SimpleAssign:
                    return GenerateCloneInitialization(propertyName);

                case InitializationKind.Clone:
                    return GenerateDictionaryInitializationWithClonedElements(propertyName, elementInfo.Type);

                case InitializationKind.Collection:
                    return GenerateDictionaryInitializationWithCollectionElements(propertyName);

                default:
                    return SyntaxFactory.EmptyStatement();
                    //throw new ArgumentException(
                    //    $"Cannot generate code for dictionary-valued property {propertyName} because dictionaries with elements of type {elementInfo.Type} are not supported.");
            }
        }

        private StatementSyntax GenerateDictionaryInitializationWithCollectionElements(string propertyName)
        {
            string argName = propertyName.ToCamelCase();
            string dictionaryElementInfoKey = PropertyInfoDictionary.MakeDictionaryItemKeyName(propertyName);
            string listElementInfoKey = PropertyInfoDictionary.MakeElementKeyName(dictionaryElementInfoKey);

            TypeSyntax dictionaryType = PropInfoDictionary.GetConcreteDictionaryType(propertyName);
            string dictionaryElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();
            ExpressionSyntax dictionaryElement = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(dictionaryElementVariableName),
                SyntaxFactory.IdentifierName(ValuePropertyName));

            string listElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();
            string collectionVariableName = _localVariableNameGenerator.GetNextDestinationVariableName();

            return SyntaxFactory.IfStatement(
                // if (foo != null)
                SyntaxHelper.IsNotNull(argName),
                SyntaxFactory.Block(
                    // Foo = new Dictionary<string, IList<D>>();
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(propertyName),
                            SyntaxFactory.ObjectCreationExpression(
                                dictionaryType,
                                SyntaxFactory.ArgumentList(),
                                default(InitializerExpressionSyntax)))),
                    // foreach (var value_0 in foo)
                    SyntaxFactory.ForEachStatement(
                        SyntaxHelper.Var(),
                        dictionaryElementVariableName,
                        SyntaxFactory.IdentifierName(argName),
                        SyntaxFactory.Block(
                            DeclareCollection(
                                PropInfoDictionary.GetConcreteListType(dictionaryElementInfoKey), collectionVariableName),
                            GenerateElementInitializationLoop(
                                listElementVariableName,
                                dictionaryElement,
                                listElementInfoKey,
                                collectionVariableName),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(propertyName),
                                        SyntaxFactory.IdentifierName(AddMethodName)),
                                    SyntaxHelper.ArgumentList(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(dictionaryElementVariableName),
                                            SyntaxFactory.IdentifierName(KeyPropertyName)),
                                        SyntaxFactory.IdentifierName(collectionVariableName))))))));
        }

        private StatementSyntax GenerateDictionaryInitializationWithClonedElements(
            string propertyName,
            TypeSyntax elementType)
        {
            string argName = propertyName.ToCamelCase();
            string valueVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();
            TypeSyntax dictionaryType = PropInfoDictionary.GetConcreteDictionaryType(propertyName);

            return SyntaxFactory.IfStatement(
                // if (foo != null)
                SyntaxHelper.IsNotNull(argName),
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        // Foo = new Dictionary<string, D>();
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(propertyName),
                            SyntaxFactory.ObjectCreationExpression(
                                dictionaryType,
                                SyntaxFactory.ArgumentList(),
                                default(InitializerExpressionSyntax)))),
                    
                    // foreach (var value_0 in foo)
                    SyntaxFactory.ForEachStatement(
                        SyntaxHelper.Var(),
                        valueVariableName,
                        SyntaxFactory.IdentifierName(argName),
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(propertyName),
                                        SyntaxFactory.IdentifierName(AddMethodName)),
                                    SyntaxHelper.ArgumentList(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(valueVariableName),
                                            SyntaxFactory.IdentifierName(KeyPropertyName)),
                                        SyntaxFactory.ObjectCreationExpression(
                                            elementType,
                                            SyntaxHelper.ArgumentList(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(valueVariableName),
                                                    SyntaxFactory.IdentifierName(ValuePropertyName))),
                                            default(InitializerExpressionSyntax)))))))));
        }

        private StatementSyntax GenerateElementInitialization( // TODO pass in initializationKind. instead of infokey.
            string elementInfoKey,
            string destinationVariableName,
            string sourceVariableName)
        {
            switch (PropInfoDictionary[elementInfoKey].InitializationKind)
            {
                case InitializationKind.SimpleAssign:
                case InitializationKind.Uri:
                    return GenerateSimpleElementInitialization(destinationVariableName, sourceVariableName);

                case InitializationKind.Clone:
                    return GenerateCloneElementInitialization(destinationVariableName, sourceVariableName, elementInfoKey);

                default:
                    return GenerateCollectionElementInitialization(destinationVariableName, sourceVariableName, elementInfoKey);
            }
        }

        private StatementSyntax GenerateSimpleElementInitialization(string destinationVariableName, string sourceVariableName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(destinationVariableName),
                        SyntaxFactory.IdentifierName(AddMethodName)),
                    SyntaxHelper.ArgumentList(
                        SyntaxFactory.IdentifierName(sourceVariableName))));
        }

        private StatementSyntax GenerateCloneElementInitialization(
            string destinationVariableName,
            string sourceVariableName,
            string elementInfoKey)
        {
            TypeSyntax elementType = PropInfoDictionary[elementInfoKey].Type;

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNull(sourceVariableName),
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(destinationVariableName),
                                SyntaxFactory.IdentifierName(AddMethodName)),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NullLiteralExpression))))),
                SyntaxFactory.ElseClause(
                    SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(destinationVariableName),
                                SyntaxFactory.IdentifierName(AddMethodName)),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.ObjectCreationExpression(
                                    elementType,
                                    SyntaxHelper.ArgumentList(
                                        SyntaxFactory.IdentifierName(sourceVariableName)),
                                    default(InitializerExpressionSyntax))))))));
        }

        private StatementSyntax GenerateCollectionElementInitialization(
            string destinationVariableName,
            string sourceVariableName,
            string elementInfoKey)
        {
            // The name of a variable used to loop over the elements of the collection
            // held in sourceVariableName.
            string collectionElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();

            // The name of the variable that holds a collection that will contain
            // copies of the elements in the source collection.
            string innerDestinationVariableName = _localVariableNameGenerator.GetNextDestinationVariableName();

            // Find out out kind of code must be generated to initialize the elements of
            // the collection.
            string sourceElementInfoKey = PropertyInfoDictionary.MakeElementKeyName(elementInfoKey);
            InitializationKind elementInitializationKind = PropInfoDictionary[sourceElementInfoKey].InitializationKind;
            TypeSyntax sourceElementType = PropInfoDictionary[elementInfoKey].Type;

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNull(SyntaxFactory.IdentifierName(sourceVariableName)),
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(destinationVariableName),
                                SyntaxFactory.IdentifierName(AddMethodName)),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NullLiteralExpression))))),
                SyntaxFactory.ElseClause(
                    SyntaxFactory.Block(
                        SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxHelper.Var(),
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier(innerDestinationVariableName),
                                        default(BracketedArgumentListSyntax),
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.ObjectCreationExpression(
                                                PropInfoDictionary.GetConcreteListType(elementInfoKey),
                                                SyntaxHelper.ArgumentList(),
                                                default(InitializerExpressionSyntax))))))),

                        SyntaxFactory.ForEachStatement(
                            SyntaxHelper.Var(),
                            collectionElementVariableName,
                            SyntaxFactory.IdentifierName(sourceVariableName),
                            SyntaxFactory.Block(
                                GenerateElementInitialization(
                                    sourceElementInfoKey,
                                    innerDestinationVariableName,
                                    collectionElementVariableName))),

                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(destinationVariableName),
                                SyntaxFactory.IdentifierName(AddMethodName)),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.IdentifierName(innerDestinationVariableName)))))));
        }

        private StatementSyntax GenerateUriInitialization(string propertyName)
        {
            PropertyInfo info = PropInfoDictionary[propertyName];
            TypeSyntax type = info.Type;

            string uriArgName = propertyName.ToCamelCase();

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNotNull(uriArgName),
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(propertyName),
                            SyntaxFactory.ObjectCreationExpression(
                                type,
                                SyntaxHelper.ArgumentList(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(uriArgName),
                                        SyntaxFactory.IdentifierName("OriginalString")),
                                            SyntaxFactory.ConditionalExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(uriArgName),
                                                    SyntaxFactory.IdentifierName("IsAbsoluteUri")),
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("UriKind"),
                                                    SyntaxFactory.IdentifierName("Absolute")),
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("UriKind"),
                                                    SyntaxFactory.IdentifierName("Relative")))),
                                default(InitializerExpressionSyntax))))));
        }
    }
}
