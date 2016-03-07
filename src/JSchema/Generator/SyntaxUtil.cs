// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    internal static class SyntaxUtil
    {
        internal static SyntaxTriviaList MakeDocCommentFromDescription(string description)
        {
            return SyntaxFactory.ParseLeadingTrivia(
@"/// <summary>
/// " + description + @"
/// </summary>
");
        }

        internal static SyntaxTriviaList MakeCopyrightComment(string copyrightNotice)
        {
            var trivia = new SyntaxTriviaList();
            if (!string.IsNullOrWhiteSpace(copyrightNotice))
            {
                trivia = trivia.AddRange(new SyntaxTrivia[]
                {
                    SyntaxFactory.Comment(copyrightNotice),
                    SyntaxFactory.Whitespace(Environment.NewLine)
                });
            }

            return trivia;
        }

        internal static AccessorDeclarationSyntax MakeGetAccessor(BlockSyntax body = null)
        {
            return MakeAccessor(SyntaxKind.GetAccessorDeclaration, body);
        }

        internal static AccessorDeclarationSyntax MakeSetAccessor(BlockSyntax body = null)
        {
            return MakeAccessor(SyntaxKind.SetAccessorDeclaration, body);
        }

        private static AccessorDeclarationSyntax MakeAccessor(SyntaxKind getOrSet, BlockSyntax body)
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
    }
}
