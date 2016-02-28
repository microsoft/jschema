// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class InterfaceGenerator : ClassOrInterfaceGenerator
    {
        private JsonSchema _rootSchema;

        public InterfaceGenerator(JsonSchema rootSchema)
        {
            _rootSchema = rootSchema;
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(string typeName)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            return SyntaxFactory.InterfaceDeclaration(typeName).WithModifiers(modifiers);
        }

        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                var propDecls = new List<PropertyDeclarationSyntax>();

                foreach (KeyValuePair<string, JsonSchema> schemaProperty in schema.Properties)
                {
                    string propertyName = schemaProperty.Key;
                    JsonSchema subSchema = schemaProperty.Value;

                    InferredType propertyType = InferTypeFromSchema(subSchema);

                    InferredType elementType = subSchema.Type == JsonType.Array
                        ? GetElementType(subSchema)
                        : InferredType.None;

                    propDecls.Add(
                        CreatePropertyDeclaration(propertyName, subSchema.Description, propertyType, elementType));
                }

                var members = SyntaxFactory.List(propDecls.Cast<MemberDeclarationSyntax>());
                TypeDeclaration = (TypeDeclaration as InterfaceDeclarationSyntax).WithMembers(members);
            }
        }

        protected override SyntaxTokenList CreatePropertyModifiers()
        {
            return default(SyntaxTokenList);
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


        // If the current schema is of array type, get the type of
        // its elements.
        // TODO: I'm not handling arrays of arrays. InferredType should encapsulate that.
        private InferredType GetElementType(JsonSchema subSchema)
        {
            return subSchema.Items != null
                ? InferTypeFromSchema(subSchema.Items)
                : new InferredType(JsonType.Object);
        }

        // Not every subschema specifies a type, but in some cases, it can be inferred.
        private InferredType InferTypeFromSchema(JsonSchema subSchema)
        {
            if (subSchema.Type == JsonType.String && subSchema.Format == FormatAttributes.DateTime)
            {
                return new InferredType("System.DateTime");
            }

            if (subSchema.Type != JsonType.None)
            {
                return new InferredType(subSchema.Type);
            }

            // If there is a reference, use the type of the reference.
            if (subSchema.Reference != null)
            {
                return InferTypeFromReference(subSchema);
            }

            // If there is an enum and every value has the same type, use that.
            object[] enumValues = subSchema.Enum;
            if (enumValues != null && enumValues.Length > 0)
            {
                var inferredType = InferTypeFromEnumValues(enumValues);
                if (inferredType != InferredType.None)
                {
                    return inferredType;
                }
            }

            return InferredType.None;
        }

        private InferredType InferTypeFromReference(JsonSchema subSchema)
        {
            if (!subSchema.Reference.IsFragment)
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorOnlyDefinitionFragmentsSupported, subSchema.Reference));
            }

            string definitionName = GetDefinitionNameFromFragment(subSchema.Reference.Fragment);

            JsonSchema definitionSchema;
            if (!_rootSchema.Definitions.TryGetValue(definitionName, out definitionSchema))
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorDefinitionDoesNotExist, definitionName));
            }

            return new InferredType(definitionName.ToPascalCase());
        }

        private static readonly Regex s_definitionRegex = new Regex(@"^#/definitions/(?<definitionName>[^/]+)$");

        private static string GetDefinitionNameFromFragment(string fragment)
        {
            Match match = s_definitionRegex.Match(fragment);
            if (!match.Success)
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorOnlyDefinitionFragmentsSupported, fragment));
            }

            return match.Groups["definitionName"].Captures[0].Value;
        }

        private static InferredType InferTypeFromEnumValues(object[] enumValues)
        {
            var jsonType = GetJsonTypeFromObject(enumValues[0]);
            for (int i = 1; i < enumValues.Length; ++i)
            {
                if (GetJsonTypeFromObject(enumValues[i]) != jsonType)
                {
                    jsonType = JsonType.None;
                    break;
                }
            }

            if (jsonType != JsonType.None)
            {
                return new InferredType(jsonType);
            }

            return InferredType.None;
        }

        private static JsonType GetJsonTypeFromObject(object obj)
        {
            if (obj is string)
            {
                return JsonType.String;
            }
            else if (obj.IsIntegralType())
            {
                return JsonType.Integer;
            }
            else if (obj.IsFloatingType())
            {
                return JsonType.Number;
            }
            else if (obj is bool)
            {
                return JsonType.Boolean;
            }
            else
            {
                return JsonType.None;
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
