// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace MountBaker.JSchema.Generator
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
        public void StartClass(string namespaceName, string className)
        {
            _namespaceName = namespaceName;
            _className = className;

            _propDecls = new List<PropertyDeclarationSyntax>();
        }

        /// <summary>
        /// Add a property to the class.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="schemaType">
        /// The JSON schema data type of the property to be added.
        /// </param>
        public void AddProperty(string propertyName, JsonType schemaType)
        {
            var modifiers = new SyntaxTokenList()
                .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            SyntaxKind typeKeyword = GetTypeKeywordFromJsonType(schemaType);

            var accessorDeclarations = new SyntaxList<AccessorDeclarationSyntax>()
                .AddRange(new AccessorDeclarationSyntax[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                });

            PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword)),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(Capitalize(propertyName)),
                SyntaxFactory.AccessorList(accessorDeclarations));

            _propDecls.Add(prop);
        }

        /// <summary>
        /// Perform any actions necessary to complete the class and generate its text.
        /// </summary>
        public void FinishClass()
        {
            var classMembers = new SyntaxList<MemberDeclarationSyntax>().AddRange(_propDecls);

            var classModifiers = new SyntaxTokenList().AddRange(new[]
            {
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            });

            ClassDeclarationSyntax classDecl = SyntaxFactory.ClassDeclaration(_className)
                .WithMembers(classMembers)
                .WithModifiers(classModifiers);

            var namespaceMembers = new SyntaxList<MemberDeclarationSyntax>().Add(classDecl);

            NamespaceDeclarationSyntax namespaceDecl = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_namespaceName))
                .WithMembers(namespaceMembers);

            var compilationUnitMembers = new SyntaxList<MemberDeclarationSyntax>().Add(namespaceDecl);

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit().WithMembers(compilationUnitMembers);

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
    }
}
