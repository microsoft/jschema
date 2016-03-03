// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private readonly string _interfaceName;

        public ClassGenerator(JsonSchema rootSchema, string interfaceName, HintDictionary hintDictionary)
            : base(rootSchema, hintDictionary)
        {
            _interfaceName = interfaceName;
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var modifiers = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            var classDeclaration = SyntaxFactory.ClassDeclaration(TypeName).WithModifiers(modifiers);

            if (_interfaceName != null)
            {
                SimpleBaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(_interfaceName));
                SeparatedSyntaxList<BaseTypeSyntax> separatedBaseList = SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType);
                BaseListSyntax baseList = SyntaxFactory.BaseList(separatedBaseList);
                classDeclaration = classDeclaration.WithBaseList(baseList);
            }

            return classDeclaration;
        }

        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                TypeDeclaration = (TypeDeclaration as ClassDeclarationSyntax)
                    .WithMembers(CreateProperties(schema))
                    .AddMembers(MakeEqualsOverride(schema));
            }
        }

        private MemberDeclarationSyntax MakeEqualsOverride(JsonSchema schema)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    "Equals")
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList(new ParameterSyntax[]
                        {
                            SyntaxFactory.Parameter(
                                default(SyntaxList<AttributeListSyntax>),
                                default(SyntaxTokenList), // modifiers
                                SyntaxFactory.ParseTypeName(TypeName),
                                SyntaxFactory.Identifier("other"),
                                default(EqualsValueClauseSyntax))
                        })))
                .WithBody(
                    SyntaxFactory.Block(MakeEqualityTests(schema)));
        }

        private StatementSyntax[] MakeEqualityTests(JsonSchema schema)
        {
            var statements = new List<StatementSyntax>();

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
                    // TODO: Compare properties.
                    // For scalars and strings: ==
                    // For objects: Object.Equals
                    // For collections: ReferenceEquals, then null check, then count check, then per property Object.Equals
                }
            }

            return statements.ToArray();
        }

        protected override SyntaxTokenList CreatePropertyModifiers()
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (_interfaceName != null)
            {
                modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            }

            return modifiers;
        }
    }
}
