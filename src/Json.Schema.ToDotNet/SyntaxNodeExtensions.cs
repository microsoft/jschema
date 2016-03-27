// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class SyntaxNodeExtensions
    {
        private const string GeneratedCodeAttributeName = "GeneratedCode";
        private static readonly string s_assemblyName = Assembly.GetCallingAssembly().GetName().Name;
        private static readonly string s_assemblyVersion = Assembly.GetCallingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Format a type declaration into a string representing the contents of a file
        /// containing that single type declaration.
        /// </summary>
        /// <param name="typeDecl">
        /// The type declaration to be formatted.
        /// </param>
        /// <param name="copyrightNotice">
        /// The copyright notice to display at the top of the file, or null if there is
        /// no copyright notice.
        /// </param>
        /// <param name="usings">
        /// A list containing the names of any namespaces required by the type declaration,
        /// or null if no namespaces are required.
        /// </param>
        /// <param name="namespaceName">
        /// The name of the namespace containing the type declaration. Required.
        /// </param>
        /// <param name="summaryComment">
        /// The summary comment for the type, or null if there is no summary comment.
        /// </param>
        /// <returns>
        /// The formatted string.
        /// </returns>
        /// <remarks>
        /// The first parameter is declared as <see cref="BaseTypeDeclarationSyntax"/>
        /// because this method works for enums as well as for classes and interfaces.
        /// Classes and interfaces derive from <see cref="TypeDeclarationSyntax"/>, but
        /// enums do not: they derive from <see cref="EnumDeclarationSyntax"/>, and both
        /// those types derive from BaseTypeDeclarationSyntax.
        /// </remarks>
        internal static string Format(
            this BaseTypeDeclarationSyntax typeDecl,
            string copyrightNotice,
            List<string> usings,
            string namespaceName,
            string summaryComment)
        {
            typeDecl = AddGeneratedCodeAttribute(typeDecl);

            if (summaryComment != null)
            {
                typeDecl = typeDecl.WithLeadingTrivia(
                    SyntaxHelper.MakeDocComment(summaryComment));
            }

            NamespaceDeclarationSyntax namespaceDecl =
                SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.IdentifierName(namespaceName))
                .AddMembers(typeDecl);

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                .AddMembers(namespaceDecl);

            if (usings == null)
            {
                usings = new List<string>();
            }

            usings.Add("System.CodeDom.Compiler"); // For GeneratedCodeAttribute

            UsingDirectiveSyntax[] usingDirectives = usings
                .Distinct()
                .OrderBy(u => u)
                .Select(u => SyntaxFactory.UsingDirective(MakeQualifiedName(u)))
                .ToArray();

            compilationUnit = compilationUnit.AddUsings(usingDirectives);

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

        private static BaseTypeDeclarationSyntax AddGeneratedCodeAttribute(BaseTypeDeclarationSyntax typeDecl)
        {
            AttributeListSyntax attributeListForGeneratedCodeAttribute =
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(MakeGeneratedCodeAttribute()));

            var enumDecl = typeDecl as EnumDeclarationSyntax;
            if (enumDecl != null)
            {
                return enumDecl.AddAttributeLists(attributeListForGeneratedCodeAttribute);
            }

            var classDecl = typeDecl as ClassDeclarationSyntax;
            if (classDecl != null)
            {
                return classDecl.AddAttributeLists(attributeListForGeneratedCodeAttribute);
            }

            var interfaceDecl = typeDecl as InterfaceDeclarationSyntax;
            if (interfaceDecl != null)
            {
                return interfaceDecl.AddAttributeLists(attributeListForGeneratedCodeAttribute);
            }

            throw new ArgumentException("Invalid argument type: " + typeDecl.GetType().Name, nameof(typeDecl));
        }

        private static AttributeSyntax MakeGeneratedCodeAttribute()
        {
            return SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName(GeneratedCodeAttributeName),
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(
                        new AttributeArgumentSyntax[]
                        {
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(s_assemblyName))),
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(s_assemblyVersion))),
                        })));
        }
    }
}
