// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class CompilationUnitExtensions
    {
        /// <summary>
        /// Format a <see cref="SyntaxNode"/> into a string.
        /// </summary>
        /// <param name="node">
        /// The <see cref="SyntaxNode"/> to be formatted.
        /// </param>
        /// <returns>
        /// The formatted string.
        /// </returns>
        internal static  string Format(this SyntaxNode node, string copyrightNotice)
        {
            if (!string.IsNullOrWhiteSpace(copyrightNotice))
            {
                node = node.WithLeadingTrivia(
                    SyntaxFactory.ParseLeadingTrivia(copyrightNotice));
            }

            var workspace = new AdhocWorkspace();
            SyntaxNode formattedNode = Formatter.Format(node, workspace);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            return sb.ToString();
        }
    }
}
