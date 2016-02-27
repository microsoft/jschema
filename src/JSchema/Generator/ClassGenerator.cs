// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generate the text of a class.
    /// </summary>
    /// <remarks>
    /// Hat tip: Mike Bennett, "Generating Code with Roslyn",
    /// https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
    /// </remarks>
    public class ClassGenerator: TypeGenerator
    {
        private List<PropertyDeclarationSyntax> _propDecls;

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

            if (_propDecls == null)
            {
                _propDecls = new List<PropertyDeclarationSyntax>();
            }

            _propDecls.Add(prop);
        }

        internal void AddEnumName(string enumName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Perform any actions necessary to complete the class and generate its text.
        /// </summary>
        public override void Finish()
        {
            var classModifiers = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            ClassDeclarationSyntax classDecl =
                SyntaxFactory.ClassDeclaration(TypeName).WithModifiers(classModifiers);

            if (_propDecls != null)
            {
                var classMembers = SyntaxFactory.List(_propDecls.Cast<MemberDeclarationSyntax>());
                classDecl = classDecl.WithMembers(classMembers);
            }

            Finish(classDecl);
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
                AddUsing(namespaceName);
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
    }
}
