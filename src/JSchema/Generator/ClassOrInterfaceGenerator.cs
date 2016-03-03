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

        public ClassOrInterfaceGenerator(JsonSchema rootSchema, HintDictionary hintDictionary)
            : base(hintDictionary)
        {
            _rootSchema = rootSchema;
        }

        protected abstract SyntaxTokenList CreatePropertyModifiers();

        protected List<MemberDeclarationSyntax> CreateProperties(JsonSchema schema)
        {
            var propDecls = new List<MemberDeclarationSyntax>();

            if (schema.Properties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> schemaProperty in schema.Properties)
                {
                    string propertyName = schemaProperty.Key;
                    JsonSchema subSchema = schemaProperty.Value;

                    propDecls.Add(
                        CreatePropertyDeclaration(propertyName, subSchema));
                }
            }

            return propDecls;
        }

        protected virtual string MakeHintDictionaryKey(string propertyName)
        {
            return TypeName + "." + propertyName.ToPascalCase();
        }

        /// <summary>
        /// Create a property declaration.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="schema">
        /// The schema that defines the type of the property.
        /// </param>
        /// <returns>
        /// A property declaration built from the specified schema.
        /// </returns>
        private PropertyDeclarationSyntax CreatePropertyDeclaration(string propertyName, JsonSchema schema)
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
                MakePropertyType(propertyName, schema),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(propertyName.ToPascalCase()),
                SyntaxFactory.AccessorList(accessorDeclarations))
                .WithLeadingTrivia(MakeDocCommentFromDescription(schema.Description));
        }

        private TypeSyntax MakePropertyType(string propertyName, JsonSchema schema)
        {
            if (IsDateTime(schema))
            {
                return MakeNamedType("System.DateTime");
            }

            if (IsUri(schema))
            {
                return MakeNamedType("System.Uri");
            }

            if (ShouldBeDictionary(propertyName, schema))
            {
                return MakeNamedType("System.Collections.Generic.Dictionary<string, string>");
            }

            string referencedEnumTypeName = GetReferencedEnumTypeName(schema);
            if (referencedEnumTypeName != null)
            {
                return MakeNamedType(referencedEnumTypeName);
            }

            switch (schema.Type)
            {
                case JsonType.Boolean:
                case JsonType.Integer:
                case JsonType.Number:
                case JsonType.String:
                    return MakePrimitiveType(schema.Type);

                case JsonType.Object:
                    return MakeObjectType(schema);

                case JsonType.Array:
                     return MakeArrayType(propertyName, schema);

                case JsonType.None:
                    JsonType inferredType = InferJsonTypeFromEnumValues(schema.Enum);
                    if (inferredType == JsonType.None)
                    {
                        inferredType = JsonType.Object;
                    }
                    return MakePrimitiveType(inferredType);

                default:
                    throw new ArgumentOutOfRangeException(nameof(schema.Type));
            }
        }

        private JsonType InferJsonTypeFromEnumValues(object[] enumValues)
        {
            JsonType jsonType = JsonType.None;

            if (enumValues != null && enumValues.Any())
            {
                jsonType = GetJsonTypeFromObject(enumValues[0]);
                for (int i = 1; i < enumValues.Length; ++i)
                {
                    if (GetJsonTypeFromObject(enumValues[i]) != jsonType)
                    {
                        jsonType = JsonType.None;
                        break;
                    }
                }
            }

            return jsonType;
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


        private bool IsDateTime(JsonSchema schema)
        {
            return schema.Type == JsonType.String && schema.Format == FormatAttributes.DateTime;
        }

        private bool IsUri(JsonSchema schema)
        {
            return schema.Type == JsonType.String && schema.Format == FormatAttributes.Uri;
        }

        private bool ShouldBeDictionary(string propertyName, JsonSchema schema)
        {
            // Ignore any DictionaryHint that might apply to this property
            // if the property is not an object.
            if (schema.Type != JsonType.Object)
            {
                return false;
            }

            // Likewise, don't make this property a dictionary if it defines
            // any properties of its own
            if (schema.Properties != null && schema.Properties.Any())
            {
                return false;
            }

            // Is there a DictionaryHint that targets this property?
            string key = MakeHintDictionaryKey(propertyName);

            return HintDictionary != null
                && HintDictionary.Any(
                    kvp => kvp.Key.Equals(key)
                    && kvp.Value.Any(hint => hint is DictionaryHint));
        }

        private string GetReferencedEnumTypeName(JsonSchema schema)
        {
            string name = null;

            if (schema.Reference != null)
            {
                string definitionName = schema.Reference.GetDefinitionName();
                if (RefersToEnumType(definitionName))
                {
                    name = definitionName;
                }
            }

            return name;
        }

        private bool RefersToEnumType(string definitionName)
        {
            // Are there any code generation hints for this definition? And if so are
            // any of them an enum hint, which means that the definition should produce
            // an enum type?
            return HintDictionary != null
                && HintDictionary.Any(
                    kvp => kvp.Key.Equals(definitionName)
                    && kvp.Value.Any(hint => hint is EnumHint));
        }

        private TypeSyntax MakePrimitiveType(JsonType jsonType)
        {
            SyntaxKind typeKeyword = GetTypeKeywordFromJsonType(jsonType);
            return SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword));
        }

        private TypeSyntax MakeObjectType(JsonSchema schema)
        {
            if (schema.Reference != null)
            {
                return MakeNamedType(schema.Reference.GetDefinitionName());
            }
            else
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
            }
        }

        private TypeSyntax MakeNamedType(string typeName)
        {
            typeName = typeName.ToPascalCase();

            string unqualifiedTypeName;
            AddUsingDirectiveForTypeName(typeName, out unqualifiedTypeName);
            return SyntaxFactory.ParseTypeName(unqualifiedTypeName);
        }

        private TypeSyntax MakeArrayType(string propertyName, JsonSchema schema)
        {
            // Create a rank-1 array of whatever this property is. If the property
            // is itself an array, this will result in a rank-2 jagged array and so on.
            return SyntaxFactory.ArrayType(
                MakePropertyType(propertyName, schema.Items),
                SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier()));
        }

        private void AddUsingDirectiveForTypeName(string className, out string unqualifiedClassName)
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
