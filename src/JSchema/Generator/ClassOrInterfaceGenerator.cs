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
            PropertyInfoDictionary = new Dictionary<string, PropertyInfo>();
        }

        protected abstract SyntaxTokenList CreatePropertyModifiers();

        /// <summary>
        /// Gets a dictionary that maps the name of each property in the generated class
        /// to a information about the property derived from the JSON schema.
        /// </summary> 
        protected Dictionary<string, PropertyInfo> PropertyInfoDictionary { get; }
        
        protected List<MemberDeclarationSyntax> GenerateProperties(JsonSchema schema)
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
        /// Synthesize a lookup key by which the elements of the specified collection-
        /// valued property can be looked up in the <see cref="PropertyInfoDictionary"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The name of a collection-valued property.
        /// </param>
        /// <returns>
        /// A lookup key for the elements of the property specified by <paramref name="propertyName"/>.
        /// </returns>
        protected string MakeElementKeyName(string propertyName)
        {
            return propertyName + "[]";
        }

        /// <summary>
        /// Gets the list of all properties declared in the schema.
        /// </summary>
        /// <remarks>
        /// Don't include information about array elements. For example, if the class has
        /// an array-valued property ArrayProp, then include "ArrayProp" in the list, but
        /// not "ArrayProp[]".
        /// </remarks>
        /// <returns>
        /// An array containing the names of the properties.
        /// </returns>
        protected string[] GetPropertyNames()
        {
            return PropertyInfoDictionary.Keys
                .Where(key => key.IndexOf("[]") == -1)
                .Select(key => key.ToPascalCase())
                .ToArray();
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
            return SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                CreatePropertyModifiers(),
                MakePropertyType(propertyName, schema),
                default(ExplicitInterfaceSpecifierSyntax),
                SyntaxFactory.Identifier(propertyName.ToPascalCase()),
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List(
                        new AccessorDeclarationSyntax[]
                        {
                            SyntaxUtil.MakeGetAccessor(),
                            SyntaxUtil.MakeSetAccessor()
                        })))
                .WithLeadingTrivia(SyntaxUtil.MakeDocComment(schema.Description));
        }

        /// <summary>
        /// Generates the appropriate <see cref="TypeSyntax" /> for the specified property.
        /// At the same time, makes a note of what kind of code will need to be generated
        /// for this property in the implementations of
        /// <see cref="IEquatable&lt;T>.Equals" /> and <see cref="Object.Equals" />.
        private TypeSyntax MakePropertyType(string propertyName, JsonSchema schema)
        {
            TypeSyntax type;

            if (IsDateTime(schema))
            {
                type = MakeNamedType("System.DateTime");

                SetPropertyInfo(
                    propertyName,
                    type,
                    ComparisonType.OperatorEquals,
                    HashType.ScalarValueType);

                return type;
            }

            if (IsUri(schema))
            {
                type = MakeNamedType("System.Uri");

                SetPropertyInfo(
                    propertyName,
                    type,
                    ComparisonType.OperatorEquals,
                    HashType.ScalarReferenceType);

                return type;
            }

            if (ShouldBeDictionary(propertyName, schema))
            {
                type = MakeNamedType("System.Collections.Generic.Dictionary<string, string>");

                SetPropertyInfo(
                    propertyName,
                    type,
                    ComparisonType.Dictionary,
                    HashType.Dictionary);

                return type;
            }

            string referencedEnumTypeName = GetReferencedEnumTypeName(schema);
            if (referencedEnumTypeName != null)
            {
                type = MakeNamedType(referencedEnumTypeName);

                SetPropertyInfo(
                    propertyName,
                    type,
                    ComparisonType.OperatorEquals,
                    HashType.ScalarValueType);

                return type;
            }

            EnumHint enumHint;
            if (ShouldBeEnum(propertyName, schema, out enumHint))
            {
                type = MakeNamedType(enumHint.TypeName);

                SetPropertyInfo(
                    propertyName,
                    type,
                    ComparisonType.OperatorEquals,
                    HashType.ScalarValueType);

                OnAdditionalType(new AdditionalTypeRequiredEventArgs(enumHint, schema));

                return type;
            }

            switch (schema.Type)
            {
                case JsonType.Boolean:
                case JsonType.Integer:
                case JsonType.Number:
                    type = MakePrimitiveType(schema.Type);

                    SetPropertyInfo(
                        propertyName,
                        type,
                        ComparisonType.OperatorEquals,
                        HashType.ScalarValueType);

                    return type;

                case JsonType.String:
                    type = MakePrimitiveType(schema.Type);

                    SetPropertyInfo(
                        propertyName,
                        type,
                        ComparisonType.OperatorEquals,
                        HashType.ScalarReferenceType);

                    return type;

                case JsonType.Object:
                    type = MakeObjectType(schema);

                    SetPropertyInfo(
                        propertyName,
                        type,
                        ComparisonType.ObjectEquals,
                        HashType.ScalarReferenceType);

                    return type;

                case JsonType.Array:
                    type = MakeArrayType(propertyName, schema);

                    SetPropertyInfo(
                        propertyName,
                        type,
                        ComparisonType.Collection,
                        HashType.Collection);

                    return type;

                case JsonType.None:
                    JsonType inferredType = InferJsonTypeFromEnumValues(schema.Enum);
                    if (inferredType == JsonType.None)
                    {
                        type = MakePrimitiveType(JsonType.Object);

                        SetPropertyInfo(
                            propertyName,
                            type,
                            ComparisonType.ObjectEquals,
                            HashType.ScalarReferenceType);

                    }
                    else if (inferredType == JsonType.String)
                    {
                        type = MakePrimitiveType(JsonType.String);

                        SetPropertyInfo(
                            propertyName,
                            type,
                            ComparisonType.OperatorEquals,
                            HashType.ScalarReferenceType);
                    }
                    else
                    {
                        type = MakePrimitiveType(inferredType);

                        SetPropertyInfo(
                            propertyName,
                            type,
                            ComparisonType.OperatorEquals,
                            HashType.ScalarValueType);
                    }

                    return type;

                default:
                    throw new ArgumentOutOfRangeException(nameof(schema.Type));
            }
        }

        private void SetPropertyInfo(
            string propertyName,
            TypeSyntax type,
            ComparisonType comparisonType,
            HashType hashType)
        {
            PropertyInfoDictionary[propertyName] = new PropertyInfo
            {
                ComparisonType = comparisonType,
                HashType = hashType,
                Type = type
            };
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

        private bool ShouldBeEnum(string propertyName, JsonSchema schema, out EnumHint enumHint)
        {
            bool shouldBeEnum = false;
            enumHint = null;

            string propertyKey = MakeHintDictionaryKey(propertyName);
            if (HintDictionary != null)
            {
                CodeGenHint[] hints;
                if (HintDictionary.TryGetValue(propertyKey, out hints))
                {
                    enumHint = hints.FirstOrDefault(hint => hint is EnumHint) as EnumHint;
                    if (enumHint != null)
                    {
                        if (string.IsNullOrWhiteSpace(enumHint.TypeName))
                        {
                            throw JSchemaException.Create(
                                Resources.ErrorEnumHintRequiresTypeName,
                                propertyKey);
                        }

                        shouldBeEnum = true;
                    }
                }
            }

            return shouldBeEnum;
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
            AddUsing("System.Collections.Generic"); // For IList.

            // Create a list of whatever this property is. If the property
            // is itself an array, this will result in a list of lists, and so on.
            return SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("IList"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                        new TypeSyntax[]
                        {
                            MakePropertyType(MakeElementKeyName(propertyName), schema.Items)
                        })));
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
