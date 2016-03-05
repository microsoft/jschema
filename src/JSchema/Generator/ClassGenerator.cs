// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generate the text of a class.
    /// </summary>
    public class ClassGenerator : ClassOrInterfaceGenerator
    {
        private readonly string _baseInterfaceName;

        // Name used for the parameters of Equals methods.
        private const string OtherParameter = "other";

        private const string CountProperty = "Count";
        private const string EqualsMethod = "Equals";
        private const string GetHashCodeMethod = "GetHashCode";
        private const string ReferenceEqualsMethod = "ReferenceEquals";
        private const string IEquatableType = "IEquatable";
        private const string ObjectType = "Object";
        private const string IntTypeAlias = "int";

        private const string TempVariableNameBase = "value_";
        private const string GetHashCodeResultVariableName = "result";

        private const int GetHashCodeSeedValue = 17;
        private const int GetHashCodeCombiningValue = 31;

        // Value used to construct unique names for each of the loop variables
        // used in the implementation of the Equals method.
        private int _variableCount = 0;

        public ClassGenerator(JsonSchema rootSchema, string interfaceName, HintDictionary hintDictionary)
            : base(rootSchema, hintDictionary)
        {
            _baseInterfaceName = interfaceName;
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(JsonSchema schema)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(TypeName)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

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

            var iEquatable = SyntaxFactory.SimpleBaseType(
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(IEquatableType),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                        new TypeSyntax[] {
                        SyntaxFactory.ParseTypeName(TypeName)
                        }))));

            baseTypes.Add(iEquatable);

            AddUsing("System"); // For IEquatable<T>

            if (baseTypes.Count > 0)
            {
                SeparatedSyntaxList<BaseTypeSyntax> separatedBaseList = SyntaxFactory.SeparatedList(baseTypes);
                BaseListSyntax baseList = SyntaxFactory.BaseList(separatedBaseList);
                classDeclaration = classDeclaration.WithBaseList(baseList);
            }

            return classDeclaration;
        }

        public override void AddMembers(JsonSchema schema)
        {
            List<MemberDeclarationSyntax> members = CreateProperties(schema);

            members.AddRange(new MemberDeclarationSyntax[]
                {
                OverrideObjectEquals(),
                OverrideGetHashCode(schema),
                ImplementIEquatableEquals(schema)
                });

            SyntaxList<MemberDeclarationSyntax> memberList = SyntaxFactory.List(members);

            TypeDeclaration = (TypeDeclaration as ClassDeclarationSyntax).WithMembers(memberList);
        }

        private MemberDeclarationSyntax OverrideObjectEquals()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                EqualsMethod)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                default(SyntaxList<AttributeListSyntax>),
                                default(SyntaxTokenList), // modifiers
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                                SyntaxFactory.Identifier(OtherParameter),
                                default(EqualsValueClauseSyntax)))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName(EqualsMethod),
                                ArgumentList(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.AsExpression,
                                        SyntaxFactory.IdentifierName(OtherParameter),
                                        SyntaxFactory.ParseTypeName(TypeName)))))));

        }
        private MemberDeclarationSyntax OverrideGetHashCode(JsonSchema schema)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                GetHashCodeMethod)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                .WithBody(
                    SyntaxFactory.Block(MakeHashCodeContributions(schema)));

        }

        private StatementSyntax[] MakeHashCodeContributions(JsonSchema schema)
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

            if (schema.Properties != null)
            {
                var uncheckedStatements = new List<StatementSyntax>();
                foreach (var property in schema.Properties)
                {
                    string comparisonTypeKey = property.Key;
                    string propName = property.Key.ToPascalCase();

                    uncheckedStatements.Add(
                        MakeHashCodeContribution(comparisonTypeKey, SyntaxFactory.IdentifierName(propName)));
                }

                statements.Add(SyntaxFactory.CheckedStatement(
                    SyntaxKind.UncheckedStatement,
                    SyntaxFactory.Block(uncheckedStatements)));
            }

            statements.Add(SyntaxFactory.ReturnStatement(
                                SyntaxFactory.IdentifierName(GetHashCodeResultVariableName)));

            return statements.ToArray();
        }

        private StatementSyntax MakeHashCodeContribution(string comparisonTypeKey, ExpressionSyntax expression)
        {
            ComparisonType comparisonType = PropertyComparisonTypeDictionary[comparisonTypeKey];
            switch (comparisonType)
            {
                case ComparisonType.OperatorEquals:
                case ComparisonType.ObjectEquals:
                    return MakeScalarHashCodeContribution(expression);

                case ComparisonType.Collection:
                    return MakeCollectionHashCodeContribution(expression);

                case ComparisonType.Dictionary:
                    return MakeDictionaryHashCodeContribution(expression); // TODO: Dictionary as array element; array element as dictionary.

                default:
                    throw new ArgumentException($"Property {comparisonTypeKey} has unknown comparison type {comparisonType}.");
            }
        }

        private StatementSyntax MakeScalarHashCodeContribution(ExpressionSyntax expression)
        {
            return SyntaxFactory.EmptyStatement();
        }

        private StatementSyntax MakeCollectionHashCodeContribution(ExpressionSyntax expression)
        {
            return SyntaxFactory.EmptyStatement();
        }

        private StatementSyntax MakeDictionaryHashCodeContribution(ExpressionSyntax expression)
        {
            return SyntaxFactory.EmptyStatement();
        }

        private MemberDeclarationSyntax ImplementIEquatableEquals(JsonSchema schema)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), EqualsMethod)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                default(SyntaxList<AttributeListSyntax>),
                                default(SyntaxTokenList), // modifiers
                                SyntaxFactory.ParseTypeName(TypeName),
                                SyntaxFactory.Identifier(OtherParameter),
                                default(EqualsValueClauseSyntax))
                        )))
                .WithBody(
                    SyntaxFactory.Block(MakeEqualityTests(schema)));
        }

        private StatementSyntax[] MakeEqualityTests(JsonSchema schema)
        {
            var statements = new List<StatementSyntax>();

            statements.Add(
                SyntaxFactory.IfStatement(
                    IsNull(SyntaxFactory.IdentifierName(OtherParameter)),
                    SyntaxFactory.Block(Return(false))));

            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    string comparisonTypeKey = property.Key;
                    string propName = property.Key.ToPascalCase();

                    statements.Add(
                        MakeComparisonTest(
                            comparisonTypeKey,
                            SyntaxFactory.IdentifierName(propName),
                            OtherPropName(propName)));
                }
            }

            // All comparisons succeeded.
            statements.Add(Return(true));

            return statements.ToArray();
        }

        private IfStatementSyntax MakeComparisonTest(
            string comparisonTypeKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
       {
            ComparisonType comparisonType = PropertyComparisonTypeDictionary[comparisonTypeKey];
            switch (comparisonType)
            {
                case ComparisonType.OperatorEquals:
                    return MakeOperatorEqualsTest(left, right);

                case ComparisonType.ObjectEquals:
                    return MakeObjectEqualsTest(left, right);

                case ComparisonType.Collection:
                    return MakeCollectionEqualsTest(comparisonTypeKey, left, right);

                case ComparisonType.Dictionary:
                    return MakeDictionaryEqualsTest(left, right); // TODO: Dictionary as array element; array element as dictionary.

                default:
                    throw new ArgumentException($"Property {comparisonTypeKey} has unknown comparison type {comparisonType}.");
            }
        }

        private IfStatementSyntax MakeOperatorEqualsTest(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.IfStatement(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    left,
                    right),
                SyntaxFactory.Block(Return(false)));
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
                        ArgumentList(left, right))),
                SyntaxFactory.Block(Return(false)));
        }

        private IfStatementSyntax MakeCollectionEqualsTest(
            string comparisonTypeLookupKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            return SyntaxFactory.IfStatement(
                // if (!Object.ReferenceEquals(Prop, other.Prop))
                AreDifferentObjects(left, right),
                SyntaxFactory.Block(
                    // if (Prop == null || other.Prop == null)
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.LogicalOrExpression,
                            IsNull(left),
                            IsNull(right)),
                        SyntaxFactory.Block(Return(false))),

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
                        SyntaxFactory.Block(Return(false))),

                    CollectionIndexLoop(comparisonTypeLookupKey, left, right)
                    ));
        }

        private ForStatementSyntax CollectionIndexLoop(
            string comparisonTypeKey,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            // The name of the index variable used in the loop over elements.
            string indexVarName = GetNextVariableName();

            // The two elements that will be compared each time through the loop.
            ExpressionSyntax leftElement =
                SyntaxFactory.ElementAccessExpression(
                    left,
                    BracketedArgumentList(
                        SyntaxFactory.IdentifierName(indexVarName)));

            ExpressionSyntax rightElement =
                SyntaxFactory.ElementAccessExpression(
                right,
                BracketedArgumentList(
                    SyntaxFactory.IdentifierName(indexVarName)));

            // From the type of the element (primitive, object, list, or dictionary), create
            // the appropriate comparison, for example, "a == b", or "Object.Equals(a, b)".
            string elementTypeLookupKeyName = comparisonTypeKey + "[]"; // TODO: DRY out propName + "[]"

            IfStatementSyntax comparisonStatement = MakeComparisonTest(elementTypeLookupKeyName, leftElement, rightElement);

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

        private string GetNextVariableName()
        {
            return TempVariableNameBase + _variableCount++;
        }

        private IfStatementSyntax MakeDictionaryEqualsTest(ExpressionSyntax left, ExpressionSyntax right)
        {
            string loopVariableName = GetNextVariableName();
            string otherPropertyVariableName = GetNextVariableName();

            return SyntaxFactory.IfStatement(
                AreDifferentObjects(left, right),
                SyntaxFactory.Block(
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.LogicalOrExpression,
                            IsNull(left),
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.LogicalOrExpression,
                                IsNull(right),
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
                        SyntaxFactory.Block(Return(false))),
                    SyntaxFactory.ForEachStatement(
                        Var(),
                        loopVariableName,
                        left,
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                default(SyntaxTokenList), // modifiers
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName("string"), // TODO: How to get the real type of the dictionary value?
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.VariableDeclarator(otherPropertyVariableName)))),
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
                                                            SyntaxFactory.IdentifierName(loopVariableName),
                                                            SyntaxFactory.IdentifierName("Key"))),
                                                    SyntaxFactory.Argument(
                                                        default(NameColonSyntax),
                                                        SyntaxFactory.Token(SyntaxKind.OutKeyword),
                                                        SyntaxFactory.IdentifierName(otherPropertyVariableName))

                                                })))),
                                SyntaxFactory.Block(Return(false))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(loopVariableName),
                                        SyntaxFactory.IdentifierName("Value")),
                                    SyntaxFactory.IdentifierName(otherPropertyVariableName)),
                                SyntaxFactory.Block(Return(false))
                                )))));
        }

#region Syntax helpers

        private TypeSyntax Var()
        {
            return SyntaxFactory.ParseTypeName("var");
        }

        private SimpleNameSyntax CountPropertyName()
        {
            return SyntaxFactory.IdentifierName(CountProperty);
        }

        protected override SyntaxTokenList CreatePropertyModifiers()
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (_baseInterfaceName != null)
            {
                modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            }

            return modifiers;
        }

        private PrefixUnaryExpressionSyntax AreDifferentObjects(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(ObjectType),
                                    SyntaxFactory.IdentifierName(ReferenceEqualsMethod)),
                                ArgumentList(left, right)));
        }

        private BinaryExpressionSyntax IsNull(ExpressionSyntax expr)
        {
            return SyntaxFactory.BinaryExpression(
                SyntaxKind.EqualsExpression,
                expr,
                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
        }

        private ArgumentListSyntax ArgumentList(params ExpressionSyntax[] args)
        {
            return SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            args.Select(arg => SyntaxFactory.Argument(arg))));

        }

        private  BracketedArgumentListSyntax BracketedArgumentList(params ExpressionSyntax[] args)
        {
            return SyntaxFactory.BracketedArgumentList(
                        SyntaxFactory.SeparatedList(
                            args.Select(arg => SyntaxFactory.Argument(arg))));
        }

        private ExpressionSyntax OtherPropName(string propName)
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(OtherParameter),
                SyntaxFactory.IdentifierName(propName));
        }

        private StatementSyntax Return(bool value)
        {
            return SyntaxFactory.ReturnStatement(
                SyntaxFactory.LiteralExpression(
                    value
                    ? SyntaxKind.TrueLiteralExpression
                    : SyntaxKind.FalseLiteralExpression));
        }

#endregion Syntax helpers
    }
}
