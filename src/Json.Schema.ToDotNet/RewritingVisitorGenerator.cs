// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
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

        private readonly string _copyrightNotice;
        private readonly string _namespaceName;
        private readonly string _className;
        private readonly string _schemaName;
        private readonly string _kindEnumName;
        private readonly string _nodeInterfaceName;

        internal RewritingVisitorGenerator(
            string copyrightNotice,
            string namespaceName,
            string className,
            string schemaName,
            string kindEnumName,
            string nodeInterfaceName)
        {
            _copyrightNotice = copyrightNotice;
            _namespaceName = namespaceName;
            _className = className;
            _schemaName = schemaName;
            _kindEnumName = kindEnumName;
            _nodeInterfaceName = nodeInterfaceName;
        }

        internal string GenerateRewritingVisitor()
        {
            ClassDeclarationSyntax visitorClassDeclaration =
                SyntaxFactory.ClassDeclaration(_className)
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.AbstractKeyword))
                    .AddMembers(
                        GenerateVisitMethod());

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
    }
}
