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
    /// <summary>
    /// Generate the text of a class.
    /// </summary>
    /// <remarks>
    /// Hat tip: Mike Bennett, "Generating Code with Roslyn",
    /// https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
    /// </remarks>
    public class ClassGenerator
    {
        private string _namespaceName;
        private string _className;
        private string _copyrightNotice;
        private List<PropertyDeclarationSyntax> _propDecls;
        private string _text;

        /// <summary>
        /// Gets the text of the generated class.
        /// </summary>
        /// <returns>
        /// A string containing the text of the generated class.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If <see cref="FinishClass"/> has not yet been called.
        /// </exception>
        public string GetText()
        {
            if (_text == null)
            {
                throw new InvalidOperationException(Resources.ErrorTextNotYetGenerated);
            }

            return _text;
        }

        /// <summary>
        /// Perform any actions necessary to begin generating the class.
        /// </summary>
        /// <param name="namespaceName">
        /// The fully qualified namespace in which the class will be placed.
        /// </param>
        /// <param name="className">
        /// The name of the class to generate.
        /// </param>
        /// <param name="copyrightNotice">
        /// The text of the copyright notice to include at the top of each file,
        /// without any comment delimiter characters.
        /// </param>
        public void StartClass(string namespaceName, string className, string copyrightNotice)
        {
            _namespaceName = namespaceName;
            _className = className;
            _copyrightNotice = copyrightNotice;

            _propDecls = new List<PropertyDeclarationSyntax>();
        }

        /// <summary>
        /// Add a property to the class.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="description">
        /// A description of the property, or <code>null</code> if there is no description.
        /// </param>
        /// <param name="schemaType">
        /// The JSON schema data type of the property to be added.
        /// </param>
        /// <param name="elementType">
        /// The JSON schema data type of the array elements of the property to be added,
        /// if the property is an array; if not, this parameter is ignored.
        /// </param>
        public void AddProperty(string propertyName, string description, JsonType schemaType, JsonType elementType)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var accessorDeclarations = SyntaxFactory.List(
                new AccessorDeclarationSyntax[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                });

            SyntaxTriviaList leadingTrivia = MakeDocCommentFromDescription(description);

            PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                MakePropertyType(schemaType, elementType),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(Capitalize(propertyName)),
                SyntaxFactory.AccessorList(accessorDeclarations))
                .WithLeadingTrivia(leadingTrivia);

            _propDecls.Add(prop);
        }

        /// <summary>
        /// Perform any actions necessary to complete the class and generate its text.
        /// </summary>
        public void FinishClass()
        {
            var classMembers = SyntaxFactory.List(_propDecls.Cast<MemberDeclarationSyntax>());

            var classModifiers = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            ClassDeclarationSyntax classDecl = SyntaxFactory.ClassDeclaration(_className)
                .WithMembers(classMembers)
                .WithModifiers(classModifiers);

            var namespaceMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDecl);

            NamespaceDeclarationSyntax namespaceDecl = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_namespaceName))
                .WithMembers(namespaceMembers);

            var compilationUnitMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceDecl);

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                .WithMembers(compilationUnitMembers)
                .WithLeadingTrivia(MakeCopyrightComment(_copyrightNotice));

            var workspace = new AdhocWorkspace();
            SyntaxNode formattedNode = Formatter.Format(compilationUnit, workspace);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            _text = sb.ToString();
        }

        private static string Capitalize(string propertyName)
        {
            return propertyName[0].ToString().ToUpperInvariant() + propertyName.Substring(1);
        }

        private TypeSyntax MakePropertyType(JsonType propertyType, JsonType elementType)
        {
            if (propertyType == JsonType.Array)
            {
                SyntaxKind elementTypeKeyword = GetTypeKeywordFromJsonType(elementType);
                return SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(elementTypeKeyword)),
                    SyntaxFactory.List(new [] { SyntaxFactory.ArrayRankSpecifier() }));
            }
            else
            {
                SyntaxKind typeKeyword = GetTypeKeywordFromJsonType(propertyType);
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword));
            }
        }

        private static readonly Dictionary<JsonType, SyntaxKind> s_jsonTypeToSyntaxKindDictionary = new Dictionary<JsonType, SyntaxKind>
        {
            [JsonType.Boolean] = SyntaxKind.BoolKeyword,
            [JsonType.Integer] = SyntaxKind.IntKeyword,
            [JsonType.Number] = SyntaxKind.DoubleKeyword,
            [JsonType.String] = SyntaxKind.StringKeyword
        };

        private static SyntaxKind GetTypeKeywordFromJsonType(JsonType type)
        {
            SyntaxKind typeKeyword;
            if (!s_jsonTypeToSyntaxKindDictionary.TryGetValue(type, out typeKeyword))
            {
                typeKeyword = SyntaxKind.ObjectKeyword;
            }

            return typeKeyword;
        }

        private SyntaxTriviaList MakeDocCommentFromDescription(string description)
        {
            var docCommentTrivia = new SyntaxTriviaList();
            if (!string.IsNullOrWhiteSpace(description))
            {
                var startTag = SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("summary"));

                var content = SyntaxFactory.XmlText(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.XmlTextLiteral(default(SyntaxTriviaList), description, description, default(SyntaxTriviaList))));

                var endTag = SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("summary"));

                var summaryElement = SyntaxFactory.XmlElement(
                    startTag,
                    SyntaxFactory.SingletonList<XmlNodeSyntax>(content),
                    endTag);

                var docComment = SyntaxFactory.DocumentationCommentTrivia(
                    SyntaxKind.MultiLineDocumentationCommentTrivia,
                    SyntaxFactory.SingletonList<XmlNodeSyntax>(summaryElement))
                    .WithLeadingTrivia(SyntaxFactory.DocumentationCommentExterior("/// "))
                    .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));

                docCommentTrivia = docCommentTrivia.Add(SyntaxFactory.Trivia(docComment));
            }

            return docCommentTrivia;
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
