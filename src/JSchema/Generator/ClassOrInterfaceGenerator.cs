// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Encapsulates the commonalities between class generation and interface generation.
    /// </summary>
    public abstract class ClassOrInterfaceGenerator : TypeGenerator
    {
        private JsonSchema _rootSchema;

        public ClassOrInterfaceGenerator(JsonSchema rootSchema)
        {
            _rootSchema = rootSchema;
        }

        protected abstract SyntaxTokenList CreatePropertyModifiers();

        protected SyntaxList<MemberDeclarationSyntax> CreateProperties(JsonSchema schema)
        {
            var propDecls = new List<PropertyDeclarationSyntax>();

            foreach (KeyValuePair<string, JsonSchema> schemaProperty in schema.Properties)
            {
                string propertyName = schemaProperty.Key;
                JsonSchema subSchema = schemaProperty.Value;

                InferredType propertyType = new InferredType(schema, subSchema);

                InferredType elementType = subSchema.Type == JsonType.Array
                    ? GetElementType(schema, subSchema)
                    : InferredType.None;

                propDecls.Add(
                    CreatePropertyDeclaration(propertyName, subSchema.Description, propertyType, elementType));
            }

            return SyntaxFactory.List(propDecls.Cast<MemberDeclarationSyntax>());
        }

        /// <summary>
        /// Create a property declaration.
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
        /// <returns>
        /// A property declaration built from the specified information.
        /// </returns>
        private PropertyDeclarationSyntax CreatePropertyDeclaration(
            string propertyName,
            string description,
            InferredType inferredPropertyType,
            InferredType inferredElementType)
        {
            var accessorDeclarations = SyntaxFactory.List(
                new AccessorDeclarationSyntax[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                });

            return SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                CreatePropertyModifiers(),
                MakePropertyType(inferredPropertyType, inferredElementType),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(propertyName.ToPascalCase()),
                SyntaxFactory.AccessorList(accessorDeclarations))
                .WithLeadingTrivia(MakeDocCommentFromDescription(description));
        }

        // If the current schema is of array type, get the type of
        // its elements.
        // TODO: I'm not handling arrays of arrays. InferredType should encapsulate that.
        private InferredType GetElementType(JsonSchema rootSchema, JsonSchema subSchema)
        {
            return subSchema.Items != null
                ? new InferredType(rootSchema, subSchema.Items)
                : new InferredType(JsonType.Object);
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
                            MakePropertyType(elementType, null), // TODO: This is where we break if the array element is itself of type array.
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
