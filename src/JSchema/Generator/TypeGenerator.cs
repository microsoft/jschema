// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.JSchema.Generator
{
    public abstract class TypeGenerator
    {
        private string _namespaceName;
        private string _copyrightNotice;
        private string _description;

        /// <summary>
        /// Gets or sets the type declaration being generated.
        /// </summary>
        protected BaseTypeDeclarationSyntax TypeDeclaration { get; set; }

        protected HashSet<string> Usings { get; private set; }

        protected string Text { get; set; }

        public abstract BaseTypeDeclarationSyntax CreateTypeDeclaration(string typeName);

        /// <summary>
        /// Adds members to the type as directed by the specified schema.
        /// </summary>
        /// <param name="schema">
        /// The JSON schema that determines which members to add to the type.
        /// </param>
        public abstract void AddMembers(JsonSchema schema);

        /// <summary>
        /// Gets the text of the generated class.
        /// </summary>
        /// <returns>
        /// A string containing the text of the generated class.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If <see cref="Finish"/> has not yet been called.
        /// </exception>
        public string GetText()
        {
            if (Text == null)
            {
                throw new InvalidOperationException(Resources.ErrorTextNotYetGenerated);
            }

            return Text;
        }

        /// <summary>
        /// Perform any actions necessary to begin generating the type.
        /// </summary>
        /// <param name="namespaceName">
        /// The fully qualified namespace in which the type will be placed.
        /// </param>
        /// <param name="typeName">
        /// The name of the type to generate.
        /// </param>
        /// <param name="description">
        /// The summary description for the type.
        /// </param>
        /// <param name="copyrightNotice">
        /// The text of the copyright notice to include at the top of each file.
        /// </param>
        public void Start(
            string namespaceName,
            string typeName,
            string copyrightNotice,
            string description)
        {
            _namespaceName = namespaceName;
            _copyrightNotice = copyrightNotice;
            _description = description;

            TypeDeclaration = CreateTypeDeclaration(typeName);
        }

        public void Finish()
        {
            TypeDeclaration = TypeDeclaration.WithLeadingTrivia(MakeDocCommentFromDescription(_description));

            var namespaceMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(TypeDeclaration);

            NamespaceDeclarationSyntax namespaceDecl = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_namespaceName))
                .WithMembers(namespaceMembers);

            var compilationUnitMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceDecl);

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit();

            if (Usings != null)
            {
                IEnumerable<UsingDirectiveSyntax> usingDirectives =
                    Usings.Select(u => SyntaxFactory.UsingDirective(MakeQualifiedName(u)));

                compilationUnit = compilationUnit.WithUsings(SyntaxFactory.List(usingDirectives));
            }

            compilationUnit = compilationUnit
                .WithMembers(compilationUnitMembers)
                .WithLeadingTrivia(MakeCopyrightComment(_copyrightNotice));

            var workspace = new AdhocWorkspace();
            SyntaxNode formattedNode = Formatter.Format(compilationUnit, workspace);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            Text = sb.ToString();
        }

        protected SyntaxTriviaList MakeDocCommentFromDescription(string description)
        {
            return SyntaxFactory.ParseLeadingTrivia(
@"/// <summary>
/// " + description + @"
/// </summary>
");
        }

        protected void AddUsing(string namespaceName)
        {
            if (Usings == null)
            {
                Usings = new HashSet<string>();
            }

            Usings.Add(namespaceName);
        }

        private NameSyntax MakeQualifiedName(string dottedName)
        {
            string[] components = dottedName.Split(new[] { '.' });
            NameSyntax qualifiedName = SyntaxFactory.ParseName(components[0]);
            for (int i = 1; i < components.Length; ++i)
            {
                qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, SyntaxFactory.IdentifierName(components[i]));
            }

            return qualifiedName;
        }

        private SyntaxTriviaList MakeCopyrightComment(string copyrightNotice)
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
