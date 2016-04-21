// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Generate the text of a class.
    /// </summary>
    public class ClassGenerator : ClassOrInterfaceGenerator
    {
        private readonly string _baseInterfaceName;
        private readonly bool _generateOverrides;
        private readonly bool _generateCloningCode;
        private readonly bool _sealClasses;
        private readonly string _syntaxInterfaceName;
        private readonly string _kindEnumName;

        // Name used for the parameters of Equals methods and copy ctor.
        private const string OtherParameter = "other";

        private const string DataContractAttributeName = "DataContract";
        private const string DataMemberAttributeName = "DataMember";
        private const string DataMemberNamePropertyName = "Name";
        private const string DataMemberIsRequiredPropertyName = "IsRequired";
        private const string DataMemberEmitDefaultValuePropertyName = "EmitDefaultValue";

        private const string CountProperty = "Count";
        private const string EqualsMethod = "Equals";
        private const string GetHashCodeMethod = "GetHashCode";
        private const string ReferenceEqualsMethod = "ReferenceEquals";
        private const string SetEqualsMethod = "SetEquals";
        private const string IEquatableType = "IEquatable";
        private const string ObjectType = "Object";
        private const string IntTypeAlias = "int";

        private const string AddMethod = "Add";
        private const string InitMethod = "Init";
        private const string DeepCloneMethod = "DeepClone";
        private const string DeepCloneCoreMethod = "DeepCloneCore";

        private const string KeyProperty = "Key";
        private const string ValueProperty = "Value";

        private const string GetHashCodeResultVariableName = "result";

        private const int GetHashCodeSeedValue = 17;
        private const int GetHashCodeCombiningValue = 31;

        private LocalVariableNameGenerator _localVariableNameGenerator;

        public ClassGenerator(
            PropertyInfoDictionary propertyInfoDictionary,
            JsonSchema schema,
            HintDictionary hintDictionary,
            string interfaceName,
            bool generateOverrides,
            bool generateCloningCode,
            bool sealClasses,
            string syntaxInterfaceName,
            string kindEnumName)
            : base(propertyInfoDictionary, schema, hintDictionary)
        {
            _baseInterfaceName = interfaceName;
            _generateOverrides = generateOverrides;
            _generateCloningCode = generateCloningCode;
            _syntaxInterfaceName = syntaxInterfaceName;
            _sealClasses = sealClasses;
            _kindEnumName = kindEnumName;

            _localVariableNameGenerator = new LocalVariableNameGenerator();
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            SyntaxKind sealedOrPartial = _sealClasses
                ? SyntaxKind.SealedKeyword
                : SyntaxKind.PartialKeyword;

            var classDeclaration = SyntaxFactory.ClassDeclaration(TypeName)
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

            var iEquatable = SyntaxFactory.SimpleBaseType(
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(IEquatableType),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                        new TypeSyntax[] {
                        SyntaxFactory.ParseTypeName(TypeName)
                        }))));

            baseTypes.Add(iEquatable);

            AddUsing("System");                         // For IEquatable<T>
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

            if (_generateCloningCode)
            {
              members.Add(GenerateSyntaxKindProperty());
            }
                
            members.AddRange(GenerateProperties());

            if (_generateOverrides)
            {
                members.AddRange(new MemberDeclarationSyntax[]
                {
                    OverrideObjectEquals(),
                    OverrideGetHashCode(),
                    ImplementIEquatableEquals()
                });
            }

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

        protected override AttributeSyntax[] CreatePropertyAttributes(string propertyName, bool isRequired)
        {
            var dataMemberAttributeArguments =
                new List<AttributeArgumentSyntax>
                {
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(DataMemberNamePropertyName),
                        default(NameColonSyntax),
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(propertyName.ToCamelCase()))),
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

            return new[]
            {
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName(DataMemberAttributeName),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(dataMemberAttributeArguments)))
            };
        }

        protected override SyntaxToken[] CreatePropertyModifiers()
        {
            return new SyntaxToken[]
                {
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                };
        }

        protected override AccessorDeclarationSyntax[] CreatePropertyAccessors()
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
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                        SyntaxHelper.MakeGetAccessor(
                            SyntaxFactory.Block(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(_kindEnumName),
                                        SyntaxFactory.IdentifierName(TypeName))))))
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(CultureInfo.CurrentCulture, Resources.SyntaxInterfaceKindDescription, _syntaxInterfaceName)));
        }

        private ConstructorDeclarationSyntax GenerateDefaultConstructor()
        {
            return SyntaxFactory.ConstructorDeclaration(TypeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBodyStatements()
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.DefaultCtorSummary,
                            TypeName)));
        }

        private ConstructorDeclarationSyntax GeneratePropertyCtor()
        {
            // Generate the argument list that will be passed from the copy ctor to the
            // Init method.
            ExpressionSyntax[] arguments = GetPropertyNames()
                .Select(name =>  SyntaxFactory.IdentifierName(name.ToCamelCase()))
                .ToArray();

            return SyntaxFactory.ConstructorDeclaration(TypeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    // This ctor takes the same parameters as the Init method, so use the
                    // same helper method to generate the parameter list.
                    GenerateInitMethodParameterList())
                .AddBodyStatements(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(InitMethod),
                            SyntaxHelper.ArgumentList(arguments))))
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.PropertyCtorSummary,
                            TypeName),
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

            foreach (string propertyName in GetPropertyNames())
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
            ExpressionSyntax[] initArguments = GetPropertyNames()
                .Select(name => 
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(OtherParameter),
                        SyntaxFactory.IdentifierName(name)))
                    .ToArray();

            return SyntaxFactory.ConstructorDeclaration(TypeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(OtherParameter))
                            .WithType(SyntaxFactory.ParseTypeName(TypeName)))
                .AddBodyStatements(
                    SyntaxFactory.IfStatement(
                        SyntaxHelper.IsNull(OtherParameter),
                        SyntaxFactory.Block(
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.ParseTypeName("ArgumentNullException"))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.IdentifierName("nameof"),
                                                SyntaxHelper.ArgumentList(
                                                    SyntaxFactory.IdentifierName(OtherParameter)))))))),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(InitMethod),
                            SyntaxHelper.ArgumentList(initArguments))))
            .WithLeadingTrivia(
                SyntaxHelper.MakeDocComment(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.CopyCtorSummary,
                        TypeName),
                    returns: null,
                    paramDescriptionDictionary: new Dictionary<string, string>
                    {
                        [OtherParameter] = Resources.CopyCtorOtherParam
                    },
                    exceptionDictionary: new Dictionary<string, string>
                    {
                        ["ArgumentNullException"] = 
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.CopyCtorOtherNullException,
                                OtherParameter)
                    }));
        }

        private MethodDeclarationSyntax GenerateISyntaxDeepClone()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(_syntaxInterfaceName),
                DeepCloneMethod)
                .WithExplicitInterfaceSpecifier(
                    SyntaxFactory.ExplicitInterfaceSpecifier(
                        SyntaxFactory.IdentifierName(_syntaxInterfaceName)))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(DeepCloneCoreMethod),
                            SyntaxHelper.ArgumentList())));
        }

        private MethodDeclarationSyntax GenerateDeepClone()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(TypeName),
                DeepCloneMethod)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.ParseTypeName(TypeName),
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName(DeepCloneCoreMethod),
                                SyntaxHelper.ArgumentList()))))
                .WithLeadingTrivia(SyntaxHelper.MakeDocComment(Resources.DeepCloneDescription));
        }

        private MethodDeclarationSyntax GenerateDeepCloneCore()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(_syntaxInterfaceName),
                DeepCloneCoreMethod)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.ParseTypeName(TypeName),
                            SyntaxHelper.ArgumentList(SyntaxFactory.ThisExpression()),
                            default(InitializerExpressionSyntax))));
        }

        private MethodDeclarationSyntax GenerateInitMethod()
        {
            _localVariableNameGenerator.Reset();

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                InitMethod)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .AddParameterListParameters(GenerateInitMethodParameterList())
                .AddBodyStatements(GenerateInitializations());
        }

        private ParameterSyntax[] GenerateInitMethodParameterList()
        {
            return GetPropertyNames()
                .Select(name => SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(name.ToCamelCase()))
                    .WithType(GetParameterListType(name)))
                .ToArray();
        }

        private StatementSyntax[] GenerateInitializations()
        {
            var statements = new List<StatementSyntax>();

            foreach (string propertyName in GetPropertyNames())
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
                SyntaxFactory.IdentifierName(ValueProperty));

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
                                        SyntaxFactory.IdentifierName(AddMethod)),
                                    SyntaxHelper.ArgumentList(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(dictionaryElementVariableName),
                                            SyntaxFactory.IdentifierName(KeyProperty)),
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
                                        SyntaxFactory.IdentifierName(AddMethod)),
                                    SyntaxHelper.ArgumentList(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(valueVariableName),
                                            SyntaxFactory.IdentifierName(KeyProperty)),
                                        SyntaxFactory.ObjectCreationExpression(
                                            elementType,
                                            SyntaxHelper.ArgumentList(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(valueVariableName),
                                                    SyntaxFactory.IdentifierName(ValueProperty))),
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
                        SyntaxFactory.IdentifierName(AddMethod)),
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
                                SyntaxFactory.IdentifierName(AddMethod)),
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
                                SyntaxFactory.IdentifierName(AddMethod)),
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
                                SyntaxFactory.IdentifierName(AddMethod)),
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
                                SyntaxFactory.IdentifierName(AddMethod)),
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

        private MemberDeclarationSyntax OverrideObjectEquals()
        {
            _localVariableNameGenerator.Reset();

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                EqualsMethod)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(OtherParameter))
                                .WithType(
                                    SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(EqualsMethod),
                            SyntaxHelper.ArgumentList(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.AsExpression,
                                    SyntaxFactory.IdentifierName(OtherParameter),
                                    SyntaxFactory.ParseTypeName(TypeName))))));

        }
        private MemberDeclarationSyntax OverrideGetHashCode()
        {
            _localVariableNameGenerator.Reset();

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                GetHashCodeMethod)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .WithBody(
                    SyntaxFactory.Block(MakeHashCodeContributions()));

        }

        private StatementSyntax[] MakeHashCodeContributions()
        {
            var statements = new List<StatementSyntax>();

            statements.Add(SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier(GetHashCodeResultVariableName),
                                        default(BracketedArgumentListSyntax),
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(GetHashCodeSeedValue))))))));

            string[] propertyNames = GetPropertyNames();
            if (propertyNames.Any())
            {
                var uncheckedStatements = new List<StatementSyntax>();
                foreach (var propertyName in propertyNames)
                {
                    uncheckedStatements.Add(
                        MakeHashCodeContribution(propertyName, SyntaxFactory.IdentifierName(propertyName)));
                }

                statements.Add(SyntaxFactory.CheckedStatement(
                    SyntaxKind.UncheckedStatement,
                    SyntaxFactory.Block(uncheckedStatements)));
            }

            statements.Add(SyntaxFactory.ReturnStatement(
                                SyntaxFactory.IdentifierName(GetHashCodeResultVariableName)));

            return statements.ToArray();
        }

        private StatementSyntax MakeHashCodeContribution(string hashKindKey, ExpressionSyntax expression)
        {
            HashKind hashKind = PropInfoDictionary[hashKindKey].HashKind;
            switch (hashKind)
            {
                case HashKind.ScalarValueType:
                    return MakeScalarHashCodeContribution(expression);

                case HashKind.ScalarReferenceType:
                    return MakeScalarReferenceTypeHashCodeContribution(expression);

                case HashKind.Collection:
                    return MakeCollectionHashCodeContribution(hashKindKey, expression);

                case HashKind.Dictionary:
                    return MakeDictionaryHashCodeContribution(expression);

                default:
                    throw new ArgumentException($"Property {hashKindKey} has unknown comparison type {hashKind}.");
            }
        }

        private StatementSyntax MakeScalarHashCodeContribution(ExpressionSyntax expression)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(GetHashCodeResultVariableName),
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.AddExpression,
                            SyntaxFactory.ParenthesizedExpression(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.MultiplyExpression,
                                    SyntaxFactory.IdentifierName(GetHashCodeResultVariableName),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(GetHashCodeCombiningValue)))),
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    expression,
                                    SyntaxFactory.IdentifierName(GetHashCodeMethod))))));
        }

        private StatementSyntax MakeScalarReferenceTypeHashCodeContribution(ExpressionSyntax expression)
        {
            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNotNull(expression),
                SyntaxFactory.Block(MakeScalarHashCodeContribution(expression)));
        }

        private StatementSyntax MakeCollectionHashCodeContribution(
            string hashKindKey,
            ExpressionSyntax expression)
        {
            string collectionElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();

            // From the type of the element (primitive, object, list, or dictionary), create
            // the appropriate hash generation code.
            string elementHashTypeKey = PropertyInfoDictionary.MakeElementKeyName(hashKindKey);

            StatementSyntax hashCodeContribution =
                MakeHashCodeContribution(
                    elementHashTypeKey,
                    SyntaxFactory.IdentifierName(collectionElementVariableName));

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNotNull(expression),
                SyntaxFactory.Block(
                    SyntaxFactory.ForEachStatement(
                        SyntaxHelper.Var(),
                        collectionElementVariableName,
                        expression,
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(GetHashCodeResultVariableName),
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.MultiplyExpression,
                                        SyntaxFactory.IdentifierName(GetHashCodeResultVariableName),
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(GetHashCodeCombiningValue))))),
                            hashCodeContribution))));
        }

        private StatementSyntax MakeDictionaryHashCodeContribution(ExpressionSyntax expression)
        {
            string xorValueVariableName = _localVariableNameGenerator.GetNextXorVariableName();
            string collectionElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();

            return SyntaxFactory.IfStatement(
                SyntaxHelper.IsNotNull(expression),
                SyntaxFactory.Block(
                    // int xor_0 = 0;
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(xorValueVariableName),
                                    default(BracketedArgumentListSyntax),
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(0)))))))
                        .WithLeadingTrivia(
                            SyntaxFactory.ParseLeadingTrivia(Resources.XorDictionaryComment)),

                    SyntaxFactory.ForEachStatement(
                        SyntaxHelper.Var(),
                        collectionElementVariableName,
                        expression,
                        SyntaxFactory.Block(
                            // xor_0 ^= value_0.Key.GetHashCode();
                            Xor(xorValueVariableName, collectionElementVariableName, KeyProperty),
                            SyntaxFactory.IfStatement(
                                SyntaxHelper.IsNotNull(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(collectionElementVariableName),
                                        SyntaxFactory.IdentifierName(ValueProperty))),
                                SyntaxFactory.Block(
                                    Xor(xorValueVariableName, collectionElementVariableName, ValueProperty))))),

                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(GetHashCodeResultVariableName),
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.AddExpression,
                                SyntaxFactory.ParenthesizedExpression(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.MultiplyExpression,
                                            SyntaxFactory.IdentifierName(GetHashCodeResultVariableName),
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(GetHashCodeCombiningValue)))),
                                SyntaxFactory.IdentifierName(xorValueVariableName))))));
        }

        private StatementSyntax Xor(string xorValueVariableName, string loopVariableName, string keyValuePairMemberName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.ExclusiveOrAssignmentExpression,
                    SyntaxFactory.IdentifierName(xorValueVariableName),
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(loopVariableName),
                                SyntaxFactory.IdentifierName(keyValuePairMemberName)),
                        SyntaxFactory.IdentifierName(GetHashCodeMethod)))));
        }

        private MemberDeclarationSyntax ImplementIEquatableEquals()
        {
            _localVariableNameGenerator.Reset();

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), EqualsMethod)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(OtherParameter))
                        .WithType(SyntaxFactory.ParseTypeName(TypeName)))
                .AddBodyStatements(GenerateEqualityTests());
        }

        private StatementSyntax[] GenerateEqualityTests()
        {
            var statements = new List<StatementSyntax>();

            statements.Add(
                SyntaxFactory.IfStatement(
                    SyntaxHelper.IsNull(OtherParameter),
                    SyntaxFactory.Block(SyntaxHelper.Return(false))));

            foreach (string propertyName in GetPropertyNames())
            {
                statements.Add(
                    MakeComparisonTest(
                        propertyName,
                        SyntaxFactory.IdentifierName(propertyName),
                        OtherPropName(propertyName)));
            }

            // All comparisons succeeded.
            statements.Add(SyntaxHelper.Return(true));

            return statements.ToArray();
        }

        private IfStatementSyntax MakeComparisonTest(
            string comparisonKindKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
       {
            ComparisonKind comparisonKind = PropInfoDictionary[comparisonKindKey].ComparisonKind;
            switch (comparisonKind)
            {
                case ComparisonKind.OperatorEquals:
                    return MakeOperatorEqualsTest(left, right);

                case ComparisonKind.ObjectEquals:
                    return MakeObjectEqualsTest(left, right);

                case ComparisonKind.Collection:
                    return MakeCollectionEqualsTest(comparisonKindKey, left, right);

                case ComparisonKind.Dictionary:
                    return MakeDictionaryEqualsTest(comparisonKindKey, left, right);

                case ComparisonKind.HashSet:
                    return MakeHashSetEqualsTest(left, right);

                default:
                    throw new ArgumentException($"Property {comparisonKindKey} has unknown comparison type {comparisonKind}.");
            }
        }

        private IfStatementSyntax MakeOperatorEqualsTest(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.IfStatement(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    left,
                    right),
                SyntaxFactory.Block(SyntaxHelper.Return(false)));
        }

        private IfStatementSyntax MakeObjectEqualsTest(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.IfStatement(
                // if (!(Object.Equals(Prop, other.Prop))
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(ObjectType),
                            SyntaxFactory.IdentifierName(EqualsMethod)),
                        SyntaxHelper.ArgumentList(left, right))),
                SyntaxFactory.Block(SyntaxHelper.Return(false)));
        }

        private IfStatementSyntax MakeCollectionEqualsTest(
            string comparisonKindKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            return SyntaxFactory.IfStatement(
                // if (!Object.ReferenceEquals(Prop, other.Prop))
                SyntaxHelper.AreDifferentObjects(left, right),
                SyntaxFactory.Block(
                    // if (Prop == null || other.Prop == null)
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.LogicalOrExpression,
                            SyntaxHelper.IsNull(left),
                            SyntaxHelper.IsNull(right)),
                        SyntaxFactory.Block(SyntaxHelper.Return(false))),

                    // if (Prop.Count != other.Prop.Count)
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                left,
                                CountPropertyName()),
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                right,
                                CountPropertyName())),
                        SyntaxFactory.Block(SyntaxHelper.Return(false))),

                    CollectionIndexLoop(comparisonKindKey, left, right)
                    ));
        }

        private ForStatementSyntax CollectionIndexLoop(
            string comparisonKindKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            // The name of the index variable used in the loop over elements.
            string indexVarName = _localVariableNameGenerator.GetNextLoopIndexVariableName();

            // The two elements that will be compared each time through the loop.
            ExpressionSyntax leftElement =
                SyntaxFactory.ElementAccessExpression(
                    left,
                    SyntaxHelper.BracketedArgumentList(
                        SyntaxFactory.IdentifierName(indexVarName)));

            ExpressionSyntax rightElement =
                SyntaxFactory.ElementAccessExpression(
                right,
                SyntaxHelper.BracketedArgumentList(
                    SyntaxFactory.IdentifierName(indexVarName)));

            // From the type of the element (primitive, object, list, or dictionary), create
            // the appropriate comparison, for example, "a == b", or "Object.Equals(a, b)".
            string elmentComparisonTypeKey = PropertyInfoDictionary.MakeElementKeyName(comparisonKindKey);

            IfStatementSyntax comparisonStatement = MakeComparisonTest(elmentComparisonTypeKey, leftElement, rightElement);

            return SyntaxFactory.ForStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(IntTypeAlias),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(indexVarName),
                            default(BracketedArgumentListSyntax),
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(0)))))),
                SyntaxFactory.SeparatedList<ExpressionSyntax>(),
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.LessThanExpression,
                    SyntaxFactory.IdentifierName(indexVarName),
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        left,
                        CountPropertyName())),
                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory.PrefixUnaryExpression(
                        SyntaxKind.PreIncrementExpression,
                        SyntaxFactory.IdentifierName(indexVarName))),
                SyntaxFactory.Block(comparisonStatement));
        }

        private IfStatementSyntax MakeDictionaryEqualsTest(
            string propertyInfoKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            string dictionaryElementVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();
            string otherPropertyVariableName = _localVariableNameGenerator.GetNextCollectionElementVariableName();

            // Construct the key into the PropertyInfoDictionary so we can look up how
            // dictionary elements are to be compared.
            string valuePropertyInfoKey = PropertyInfoDictionary.MakeDictionaryItemKeyName(propertyInfoKey);
            TypeSyntax dictionaryValueType = PropInfoDictionary[valuePropertyInfoKey].Type;

            return SyntaxFactory.IfStatement(
                // if (!Object.ReferenceEquals(left, right))
                SyntaxHelper.AreDifferentObjects(left, right),
                SyntaxFactory.Block(
                    // if (left == null || right == null || left.Count != right.Count)
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.LogicalOrExpression,
                            SyntaxHelper.IsNull(left),
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.LogicalOrExpression,
                                SyntaxHelper.IsNull(right),
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        left,
                                        CountPropertyName()),
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        right,
                                        CountPropertyName())))),
                        // return false;
                        SyntaxFactory.Block(SyntaxHelper.Return(false))),
                    // foreach (var value_0 in left)
                    SyntaxFactory.ForEachStatement(
                        SyntaxHelper.Var(),
                        dictionaryElementVariableName,
                        left,
                        SyntaxFactory.Block(
                            // var value_1;
                            SyntaxFactory.LocalDeclarationStatement(
                                default(SyntaxTokenList), // modifiers
                                SyntaxFactory.VariableDeclaration(
                                    dictionaryValueType,
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.VariableDeclarator(otherPropertyVariableName)))),
                            // if (!right.TryGetValue(value_0.Key, out value_1))
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.PrefixUnaryExpression(
                                    SyntaxKind.LogicalNotExpression,
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            right,
                                            SyntaxFactory.IdentifierName("TryGetValue")),
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList(
                                                new ArgumentSyntax[]
                                                {
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.IdentifierName(dictionaryElementVariableName),
                                                            SyntaxFactory.IdentifierName(KeyProperty))),
                                                    SyntaxFactory.Argument(
                                                        default(NameColonSyntax),
                                                        SyntaxFactory.Token(SyntaxKind.OutKeyword),
                                                        SyntaxFactory.IdentifierName(otherPropertyVariableName))

                                                })))),
                                // return false;
                                SyntaxFactory.Block(SyntaxHelper.Return(false))),

                            MakeComparisonTest(
                                    valuePropertyInfoKey,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(dictionaryElementVariableName),
                                        SyntaxFactory.IdentifierName(ValueProperty)),
                                    SyntaxFactory.IdentifierName(otherPropertyVariableName))))));
        }

        private IfStatementSyntax MakeHashSetEqualsTest(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.IfStatement(
                // if (!Object.ReferenceEquals(Prop, other.Prop))
                SyntaxHelper.AreDifferentObjects(left, right),
                SyntaxFactory.Block(
                    // if (Prop == null || other.Prop == null)
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.LogicalOrExpression,
                            SyntaxHelper.IsNull(left),
                            SyntaxHelper.IsNull(right)),
                        SyntaxFactory.Block(SyntaxHelper.Return(false))),

                    SyntaxFactory.IfStatement(
                        SyntaxFactory.PrefixUnaryExpression(
                            SyntaxKind.LogicalNotExpression,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    left,
                                    SyntaxFactory.IdentifierName(SetEqualsMethod)),
                                SyntaxHelper.ArgumentList(right))),
                        SyntaxFactory.Block(SyntaxHelper.Return(false)))));
        }

        #region Syntax helpers

        private SimpleNameSyntax CountPropertyName()
        {
            return SyntaxFactory.IdentifierName(CountProperty);
        }

        private ExpressionSyntax OtherPropName(string propName)
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(OtherParameter),
                SyntaxFactory.IdentifierName(propName));
        }

        #endregion Syntax helpers
    }
}
