// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal class RewritingVisitorGenerator
    {
        private const string NodeParameterName = "node";
        private const string VisitMethodName = "Visit";
        private const string VisitActualMethodName = "VisitActual";
        private const string VisitNullCheckedMethodName = "VisitNullChecked";
        private const string TypeParameterName = "T";

        private readonly string _copyrightNotice;
        private readonly string _namespaceName;
        private readonly string _className;
        private readonly string _schemaName;
        private readonly string _kindEnumName;
        private readonly string _nodeInterfaceName;
        private readonly List<string> _generatedClassNames;

        internal RewritingVisitorGenerator(
            string copyrightNotice,
            string namespaceName,
            string className,
            string schemaName,
            string kindEnumName,
            string nodeInterfaceName,
            IEnumerable<string> generatedClassNames)
        {
            _copyrightNotice = copyrightNotice;
            _namespaceName = namespaceName;
            _className = className;
            _schemaName = schemaName;
            _kindEnumName = kindEnumName;
            _nodeInterfaceName = nodeInterfaceName;
            _generatedClassNames = generatedClassNames.OrderBy(gn => gn).ToList();
        }

        internal string GenerateRewritingVisitor()
        {
            ClassDeclarationSyntax visitorClassDeclaration =
                SyntaxFactory.ClassDeclaration(_className)
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.AbstractKeyword))
                    .AddMembers(
                        GenerateVisitMethod(),
                        GenerateVisitActualMethod(),
                        GenerateVisitNullCheckedMethod())
                    .AddMembers(
                        GenerateVisitClassMethods());

            var usings = new List<string> { "System" };

            string summaryComment = string.Format(
                CultureInfo.CurrentCulture,
                Resources.RewritingVisitorSummary,
                _schemaName);

            return visitorClassDeclaration.Format(
                _copyrightNotice,
                usings,
                _namespaceName,
                summaryComment);
        }

        private MemberDeclarationSyntax GenerateVisitMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                VisitMethodName)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier(NodeParameterName))
                        .WithType(
                            SyntaxFactory.ParseTypeName(_nodeInterfaceName)))
                .AddBodyStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxFactory.IdentifierName(VisitActualMethodName)))
                            .AddArgumentListArguments(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName(NodeParameterName)))))
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.RewritingVisitorVisitMethodSummary,
                            _schemaName),
                        Resources.RewritingVisitorVisitMethodReturns,
                        new Dictionary<string, string>
                        {
                            [NodeParameterName] = Resources.RewritingVisitorVisitMethodNodeParameter
                        }));
        }

        private MemberDeclarationSyntax GenerateVisitActualMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                VisitActualMethodName)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier(NodeParameterName))
                        .WithType(
                            SyntaxFactory.ParseTypeName(_nodeInterfaceName)))
                .AddBodyStatements(
                    SyntaxFactory.IfStatement(
                        SyntaxHelper.IsNull(NodeParameterName),
                        SyntaxFactory.Block(
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.ParseTypeName("ArgumentNullException"),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    SyntaxFactory.Literal(NodeParameterName))))),
                                    default(InitializerExpressionSyntax))))),
                    SyntaxFactory.SwitchStatement(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NodeParameterName),
                            SyntaxFactory.IdentifierName(_kindEnumName)))
                            .AddSections(
                                GenerateVisitActualSwitchSections()))
                .WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.RewritingVisitorVisitActualMethodSummary,
                            _schemaName),
                        Resources.RewritingVisitorVisitActualMethodReturns,
                        new Dictionary<string, string>
                        {
                            [NodeParameterName] = Resources.RewritingVisitorVisitActualMethodNodeParameter
                        }));
        }

        private SwitchSectionSyntax[] GenerateVisitActualSwitchSections()
        {
            // There is one switch section for each generated class, plus one for the default.
            var switchSections = new SwitchSectionSyntax[_generatedClassNames.Count + 1];

            int index = 0;
            foreach (string generatedClassName in _generatedClassNames)
            {
                string methodName = MakeVisitClassMethodName(generatedClassName);

                switchSections[index++] = SyntaxFactory.SwitchSection(
                    SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                        SyntaxFactory.CaseSwitchLabel(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(_kindEnumName),
                                SyntaxFactory.IdentifierName(generatedClassName)))),
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName(methodName),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.CastExpression(
                                                SyntaxFactory.ParseTypeName(generatedClassName),
                                                SyntaxFactory.IdentifierName(NodeParameterName)))))))));
            }

            switchSections[index] = SyntaxFactory.SwitchSection(
                SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                    SyntaxFactory.DefaultSwitchLabel()),
                SyntaxFactory.SingletonList<StatementSyntax>(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.IdentifierName(NodeParameterName))));

            return switchSections;
        }

        private MethodDeclarationSyntax GenerateVisitNullCheckedMethod()
        {
            TypeSyntax typeParameterType = SyntaxFactory.ParseTypeName(TypeParameterName);

            return SyntaxFactory.MethodDeclaration(
                typeParameterType,
                VisitNullCheckedMethodName)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .AddTypeParameterListParameters(
                    SyntaxFactory.TypeParameter(TypeParameterName))
                .AddConstraintClauses(
                    SyntaxFactory.TypeParameterConstraintClause(
                        SyntaxFactory.IdentifierName(TypeParameterName),
                        SyntaxFactory.SeparatedList(
                            new TypeParameterConstraintSyntax[]
                            {
                                SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint),
                                SyntaxFactory.TypeConstraint(
                                    SyntaxFactory.ParseTypeName(_nodeInterfaceName))
                            })))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(NodeParameterName))
                        .WithType(typeParameterType))
                .AddBodyStatements(
                    SyntaxFactory.IfStatement(
                        SyntaxHelper.IsNull(NodeParameterName),
                        SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))),
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.CastExpression(
                            typeParameterType,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName(VisitMethodName),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName(NodeParameterName))))))));
        }

        private MemberDeclarationSyntax[] GenerateVisitClassMethods()
        {
            // There is one VisitXxx method for each generated class.
            var visitClassMethods = new MemberDeclarationSyntax[_generatedClassNames.Count];

            int index = 0;
            foreach (string generatedClassName in _generatedClassNames)
            {
                visitClassMethods[index++] = GenerateVisitClassMethod(generatedClassName);
            }

            return visitClassMethods;
        }

        private MethodDeclarationSyntax GenerateVisitClassMethod(string generatedClassName)
        {
            string methodName = MakeVisitClassMethodName(generatedClassName);
            TypeSyntax generatedClassType = SyntaxFactory.ParseTypeName(generatedClassName);

            return SyntaxFactory.MethodDeclaration(generatedClassType, methodName)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(NodeParameterName))
                    .WithType(generatedClassType))
                .AddBodyStatements(
                    SyntaxFactory.IfStatement(
                        SyntaxHelper.IsNotNull(NodeParameterName),
                        SyntaxFactory.Block()),
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.IdentifierName(NodeParameterName)));
        }

        private string MakeVisitClassMethodName(string className)
        {
            return VisitMethodName + className;
        }
    }
}
