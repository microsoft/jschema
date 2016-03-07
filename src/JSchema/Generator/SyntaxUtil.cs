// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
    }
}
