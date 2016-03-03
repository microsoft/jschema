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
    /// <remarks>
    /// Hat tip: Mike Bennett, "Generating Code with Roslyn",
    /// https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
    /// </remarks>
    public class ClassGenerator : ClassOrInterfaceGenerator
    {
        private readonly string _baseInterfaceName;

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
                    SyntaxFactory.Identifier("IEquatable"),
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
                ImplementIEquatableEquals(schema)
                });

            SyntaxList<MemberDeclarationSyntax> memberList = SyntaxFactory.List(members);

            TypeDeclaration = (TypeDeclaration as ClassDeclarationSyntax).WithMembers(memberList);
        }

        private MemberDeclarationSyntax OverrideObjectEquals()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                "Equals")
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
                                SyntaxFactory.Identifier("other"),
                                default(EqualsValueClauseSyntax)))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName("Equals"),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.AsExpression,
                                                SyntaxFactory.IdentifierName("other"),
                                                SyntaxFactory.ParseTypeName(TypeName)))))))));

        }

        private MemberDeclarationSyntax ImplementIEquatableEquals(JsonSchema schema)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "Equals")
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
                                SyntaxFactory.Identifier("other"),
                                default(EqualsValueClauseSyntax))
                        )))
                .WithBody(
                    SyntaxFactory.Block(MakeEqualityTests(schema)));
        }

        private StatementSyntax[] MakeEqualityTests(JsonSchema schema)
        {
            var statements = new List<StatementSyntax>();

            // First, check "other" against null.
            statements.Add(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.IdentifierName("other"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                    SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))))
                );

            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    string propName = property.Key.ToPascalCase();
                    switch (PropertyComparisonTypeDictionary[propName.ToCamelCase()])
                    {
                        case ComparisonType.OperatorEquals:
                            statements.Add(MakeOperatorEqualsTest(propName));
                            break;

                        case ComparisonType.ObjectEquals:
                            statements.Add(MakeObjectEqualsTest(propName));
                            break;

                        case ComparisonType.Collection:
                            // For collections: ReferenceEquals, then null check, then count check, then per property Object.Equals
                            break;

                        default:
                            break;
                    }
                }
            }

            // All comparisons succeeded.
            statements.Add(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));

            return statements.ToArray();
        }

        private StatementSyntax MakeOperatorEqualsTest(string propName)
        {
            return SyntaxFactory.IfStatement(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    SyntaxFactory.IdentifierName(propName),
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("other"),
                        SyntaxFactory.IdentifierName(propName))),
                SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
        }

        private StatementSyntax MakeObjectEqualsTest(string propName)
        {
            return SyntaxFactory.IfStatement(
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Object"),
                            SyntaxFactory.IdentifierName("Equals")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new[]
                                {
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.IdentifierName(propName)),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("other"),
                                            SyntaxFactory.IdentifierName(propName)))
                                })))),
                SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
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
    }
}
