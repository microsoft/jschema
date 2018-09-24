// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// The methods in this class encapsulate or simplify the generation of certain
    /// code patterns that The ToDotNet code generator emits.
    /// </summary>
    internal static class SyntaxHelper
    {
        private const string DocCommentSummaryFormat =
@"/// <summary>
/// {0}
/// </summary>
";
        private const string DocCommentParamFormat =
@"/// <param name=""{0}"">
/// {1}
/// </param>
";

        private const string DocCommentReturnsFormat =
@"/// <returns>
/// {0}
/// </returns>
";
        private const string DocCommentExceptionFormat =
@"/// <exception cref=""{0}"">
/// {1}
/// </exception>
";

        internal static SyntaxTriviaList MakeDocComment(
            string summary,
            string returns = null,
            Dictionary<string, string> paramDescriptionDictionary = null,
            Dictionary<string, string> exceptionDictionary = null)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(summary))
            {
                sb.AppendFormat(
                    CultureInfo.CurrentCulture,
                    DocCommentSummaryFormat,
                    summary);
            }

            if (paramDescriptionDictionary != null)
            {
                foreach (KeyValuePair<string, string> kvp in paramDescriptionDictionary)
                {
                    sb.AppendFormat(
                        CultureInfo.CurrentCulture,
                        DocCommentParamFormat,
                        kvp.Key,
                        kvp.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(returns))
            {
                sb.AppendFormat(
                    CultureInfo.CurrentCulture,
                    DocCommentReturnsFormat,
                    returns);
            }

            if (exceptionDictionary != null)
            {
                foreach (KeyValuePair<string, string> kvp in exceptionDictionary)
                {
                    sb.AppendFormat(
                        CultureInfo.CurrentCulture,
                        DocCommentExceptionFormat,
                        kvp.Key,
                        kvp.Value);
                }
            }

            return SyntaxFactory.ParseLeadingTrivia(sb.ToString());
        }

        internal static AccessorDeclarationSyntax MakeGetAccessor(BlockSyntax body = null)
        {
            return MakeAccessor(SyntaxKind.GetAccessorDeclaration, body);
        }

        internal static AccessorDeclarationSyntax MakeSetAccessor(BlockSyntax body = null)
        {
            return MakeAccessor(SyntaxKind.SetAccessorDeclaration, body);
        }

        internal static AccessorDeclarationSyntax MakeAccessor(SyntaxKind getOrSet, BlockSyntax body)
        {
            return SyntaxFactory.AccessorDeclaration(
                getOrSet,
                default(SyntaxList<AttributeListSyntax>),
                default(SyntaxTokenList),
                getOrSet == SyntaxKind.GetAccessorDeclaration
                    ? SyntaxFactory.Token(SyntaxKind.GetKeyword)
                    : SyntaxFactory.Token(SyntaxKind.SetKeyword),
                body,
                body == null ? SyntaxFactory.Token(SyntaxKind.SemicolonToken) : default(SyntaxToken));
        }

        internal static PrefixUnaryExpressionSyntax AreDifferentObjects(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(WellKnownTypeNames.Object),
                                    SyntaxFactory.IdentifierName(WellKnownMethodNames.ReferenceEqualsMethod)),
                                ArgumentList(left, right)));
        }

        internal static LiteralExpressionSyntax Null()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        internal static BinaryExpressionSyntax IsNull(string identifierName)
        {
            return IsNull(SyntaxFactory.IdentifierName(identifierName));
        }

        internal static BinaryExpressionSyntax IsNull(ExpressionSyntax expr)
        {
            return SyntaxFactory.BinaryExpression(
                SyntaxKind.EqualsExpression,
                expr,
                Null());
        }

        internal static BinaryExpressionSyntax IsNotNull(string identifierName)
        {
            return IsNotNull(SyntaxFactory.IdentifierName(identifierName));
        }

        internal static BinaryExpressionSyntax IsNotNull(ExpressionSyntax expr)
        {
            return SyntaxFactory.BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                expr,
                Null());
        }

        internal static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] args)
        {
            return SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            args.Select(arg => SyntaxFactory.Argument(arg))));

        }

        internal static BracketedArgumentListSyntax BracketedArgumentList(params ExpressionSyntax[] args)
        {
            return SyntaxFactory.BracketedArgumentList(
                        SyntaxFactory.SeparatedList(
                            args.Select(arg => SyntaxFactory.Argument(arg))));
        }

        internal static StatementSyntax Return(bool value)
        {
            return SyntaxFactory.ReturnStatement(
                SyntaxFactory.LiteralExpression(
                    value
                    ? SyntaxKind.TrueLiteralExpression
                    : SyntaxKind.FalseLiteralExpression));
        }

        internal static StatementSyntax Return(int value)
        {
            return SyntaxFactory.ReturnStatement(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(value)));
        }

        internal static TypeSyntax Var()
        {
            return SyntaxFactory.ParseTypeName("var");
        }

        // Roslyn doesn't directly expose a "nameof expression". This article shows how
        // to make one:
        // https://stackoverflow.com/questions/46259039/constructing-nameof-expression-via-syntaxfactory-roslyn
        internal static ExpressionSyntax NameofExpression(string symbol)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier(
                        leading: SyntaxFactory.TriviaList(),
                        contextualKind: SyntaxKind.NameOfKeyword,
                        text: "nameof",
                        valueText: "nameof",
                        trailing: SyntaxFactory.TriviaList())),
                ArgumentList(
                    SyntaxFactory.IdentifierName(symbol)));
        }

        private static readonly TypeSyntax ArgumentNullExceptionType = SyntaxFactory.ParseTypeName(nameof(ArgumentNullException));

        internal static ObjectCreationExpressionSyntax NewArgumentNullException(string parameterName)
        {
            return SyntaxFactory.ObjectCreationExpression(
                ArgumentNullExceptionType,
                ArgumentList(
                    NameofExpression(parameterName)),
                default(InitializerExpressionSyntax));
        }

        private static readonly TypeSyntax InvalidOperationExceptionType = SyntaxFactory.ParseTypeName(nameof(InvalidOperationException));

        internal static ObjectCreationExpressionSyntax NewInvalidOperationException()
        {
            return SyntaxFactory.ObjectCreationExpression(
                InvalidOperationExceptionType,
                ArgumentList(),
                default(InitializerExpressionSyntax));
        }

        internal static IfStatementSyntax NullParameterCheck(string parameterName)
        {
            return SyntaxFactory.IfStatement(
                IsNull(parameterName),
                SyntaxFactory.Block(
                    SyntaxFactory.ThrowStatement(
                        NewArgumentNullException(parameterName))));
        }
    }
}
