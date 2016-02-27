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
    public enum Foo
    {
        None,
        Bax
    }
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
        private HintDictionary _hintDictionary;
        private List<PropertyDeclarationSyntax> _propDecls;
        private HashSet<string> _usings;
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
        public void StartClass(
            string namespaceName,
            string className,
            string copyrightNotice,
            HintDictionary hintDictionary)
        {
            _namespaceName = namespaceName;
            _className = className;
            _copyrightNotice = copyrightNotice;
            _hintDictionary = hintDictionary;

            _propDecls = new List<PropertyDeclarationSyntax>();
            _usings = new HashSet<string>();
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
        /// <param name="inferredPropertyType">
        /// The inferred type of the property to be added.
        /// </param>
        /// <param name="inferredElementType">
        /// The inferred type of the array elements of the property to be added,
        /// if the property is an array; if not, this parameter is ignored.
        /// </param>
        public void AddProperty(string propertyName, string description, InferredType inferredPropertyType, InferredType inferredElementType)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var accessorDeclarations = SyntaxFactory.List(
                new AccessorDeclarationSyntax[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                });

            PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                MakePropertyType(inferredPropertyType, inferredElementType),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(propertyName.ToPascalCase()),
                SyntaxFactory.AccessorList(accessorDeclarations))
                .WithLeadingTrivia(MakeDocCommentFromDescription(description));

            _propDecls.Add(prop);
        }

        internal void AddEnumName(string enumName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Perform any actions necessary to complete the class and generate its text.
        /// </summary>
        public void FinishClass(string description)
        {
            var classMembers = SyntaxFactory.List(_propDecls.Cast<MemberDeclarationSyntax>());

            var classModifiers = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            ClassDeclarationSyntax classDecl = SyntaxFactory.ClassDeclaration(_className)
                .WithMembers(classMembers)
                .WithModifiers(classModifiers)
                .WithLeadingTrivia(MakeDocCommentFromDescription(description));

            var namespaceMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDecl);

            NamespaceDeclarationSyntax namespaceDecl = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_namespaceName))
                .WithMembers(namespaceMembers);

            var compilationUnitMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceDecl);

            IEnumerable<UsingDirectiveSyntax> usingDirectives =
                _usings.Select(u => SyntaxFactory.UsingDirective(MakeQualifiedName(u)));

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                .WithUsings(SyntaxFactory.List(usingDirectives))
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

        private TypeSyntax MakePropertyType(InferredType propertyType, InferredType elementType)
        {
            switch (propertyType.Kind)
            {
                case InferredTypeKind.JsonType:
                    JsonType jsonType = propertyType.GetJsonType();
                    if (jsonType == JsonType.Array)
                    {
                        return SyntaxFactory.ArrayType(
                            MakePropertyType(elementType, null),
                            SyntaxFactory.List(new[] { SyntaxFactory.ArrayRankSpecifier() }));
                    }

                    SyntaxKind typeKeyword = GetTypeKeywordFromJsonType(propertyType.GetJsonType());
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword));

                case InferredTypeKind.ClassName:
                    string className = propertyType.GetClassName();
                    string unqualifiedClassName;
                    AddUsingDirectiveForClassName(className, out unqualifiedClassName);
                    return SyntaxFactory.ParseTypeName(unqualifiedClassName);

                case InferredTypeKind.None:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType));
            }
        }

        private void AddUsingDirectiveForClassName(string className, out string unqualifiedClassName)
        {

            int index = className.LastIndexOf('.');
            if (index != -1)
            {
                unqualifiedClassName = className.Substring(index + 1);
                string namespaceName = className.Substring(0, index);
                _usings.Add(namespaceName);
            }
            else
            {
                unqualifiedClassName = className;
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
            return SyntaxFactory.ParseLeadingTrivia(
@"/// <summary>
/// " + description + @"
/// </summary>
");
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
