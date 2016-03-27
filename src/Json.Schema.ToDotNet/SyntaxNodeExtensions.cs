// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class SyntaxNodeExtensions
    {

        /// <summary>
        /// Format a type declaration into a string representing the contents of a file
        /// containing that single type declaration.
        /// </summary>
        /// <param name="typeDecl">
        /// The type declaration to be formatted.
        /// </param>
        /// <param name="namespaceName">
        /// The name of the namespace containing the type declaration.
        /// </param>
        /// <param name="usings">
        /// The names of any namespaces required by the type declaration.
        /// </param>
        /// <param name="copyrightNotice">
        /// The copyright notice to display at the top of the file.
        /// </param>
        /// <returns>
        /// The formatted string.
        /// </returns>
        internal static string Format(
            this BaseTypeDeclarationSyntax typeDecl,
            string namespaceName,
            IEnumerable<string> usings,
            string copyrightNotice)
        {
            NamespaceDeclarationSyntax namespaceDecl =
                SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.IdentifierName(namespaceName))
                .AddMembers(typeDecl);

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                .AddMembers(namespaceDecl);

            if (usings != null)
            {
                UsingDirectiveSyntax[] usingDirectives = usings
                    .OrderBy(u => u)
                    .Select(u => SyntaxFactory.UsingDirective(MakeQualifiedName(u)))
                    .ToArray();

                compilationUnit = compilationUnit.AddUsings(usingDirectives);
            }

            if (!string.IsNullOrWhiteSpace(copyrightNotice))
            {
                compilationUnit = compilationUnit.WithLeadingTrivia(
                    SyntaxFactory.ParseLeadingTrivia(copyrightNotice));
            }

            var workspace = new AdhocWorkspace();
            SyntaxNode formattedNode = Formatter.Format(compilationUnit, workspace);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            return sb.ToString();
        }

        private static NameSyntax MakeQualifiedName(string dottedName)
        {
            string[] components = dottedName.Split(new[] { '.' });
            NameSyntax qualifiedName = SyntaxFactory.ParseName(components[0]);
            for (int i = 1; i < components.Length; ++i)
            {
                qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, SyntaxFactory.IdentifierName(components[i]));
            }

            return qualifiedName;
        }
    }
}
