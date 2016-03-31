// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Contains information necessary to generate code for each property in the class
    /// described by a schema.
    /// </summary>
    internal class PropertyInfoDictionary : IReadOnlyDictionary<string, PropertyInfo>
    {
        private readonly string _typeName;
        private readonly JsonSchema _schema;
        private readonly HintDictionary _hintDictionary;

        private readonly ImmutableDictionary<string, PropertyInfo> _dictionary;

        private static readonly Dictionary<JsonType, SyntaxKind> s_jsonTypeToSyntaxKindDictionary = new Dictionary<JsonType, SyntaxKind>
        {
            [JsonType.Boolean] = SyntaxKind.BoolKeyword,
            [JsonType.Integer] = SyntaxKind.IntKeyword,
            [JsonType.Number] = SyntaxKind.DoubleKeyword,
            [JsonType.String] = SyntaxKind.StringKeyword
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoDictionary"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The name of the type described by <paramref name="schema"/>.
        /// </param>
        /// <param name="schema">
        /// A schema describing each property in the class.
        /// </param>
        /// <param name="hintDictionary">
        /// A dictionary of hints to guide code generation.
        /// </param>
        internal PropertyInfoDictionary(string typeName, JsonSchema schema, HintDictionary hintDictionary)
        {
            _typeName = typeName;
            _schema = schema;
            _hintDictionary = hintDictionary;

            _dictionary = PropertyInfoDictionaryFromSchema();
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
        public static string MakeElementKeyName(string propertyName)
        {
            return propertyName + "[]";
        }

        public static SyntaxKind GetTypeKeywordFromJsonType(JsonType type)
        {
            SyntaxKind typeKeyword;
            if (!s_jsonTypeToSyntaxKindDictionary.TryGetValue(type, out typeKeyword))
            {
                typeKeyword = SyntaxKind.ObjectKeyword;
            }

            return typeKeyword;
        }
        #region IReadOnlyDictionary

        public PropertyInfo this[string key] => _dictionary[key];

        public int Count => _dictionary.Count;

        public IEnumerable<string> Keys => _dictionary.Keys;

        public IEnumerable<PropertyInfo> Values => _dictionary.Values;

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, PropertyInfo>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool TryGetValue(string key, out PropertyInfo value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        #endregion IReadOnlyDictionary

        private ImmutableDictionary<string, PropertyInfo> PropertyInfoDictionaryFromSchema()
        {
            var entries = new List<KeyValuePair<string, PropertyInfo>>();

            if (_schema.Properties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> schemaProperty in _schema.Properties)
                {
                    string propertyName = schemaProperty.Key;
                    JsonSchema subSchema = schemaProperty.Value;

                    AddPropertyInfoFromPropertySchema(entries, propertyName, subSchema);
                }
            }

            return ImmutableDictionary.CreateRange(entries);
        }

        private void AddPropertyInfoFromPropertySchema(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string propertyName,
            JsonSchema propertySchema)
        {
            if (propertySchema.IsDateTime())
            {
                AddPropertyInfo(
                    entries,
                    propertyName,
                    ComparisonKind.OperatorEquals,
                    HashKind.ScalarValueType,
                    InitializationKind.SimpleAssign,
                    MakeNamedType("System.DateTime"));
            }

            if (propertySchema.IsUri())
            {
                AddPropertyInfo(
                    entries,
                    propertyName,
                    ComparisonKind.OperatorEquals,
                    HashKind.ScalarReferenceType,
                    InitializationKind.Uri,
                    MakeNamedType("System.Uri"));
            }

            if (propertySchema.ShouldBeDictionary(_typeName, propertyName, _hintDictionary))
            {
                AddPropertyInfo(
                    entries,
                    propertyName,
                    ComparisonKind.Dictionary,
                    HashKind.Dictionary,
                    InitializationKind.Clone,
                    MakeNamedType("System.Collections.Generic.Dictionary<string, string>"));
            }

            string referencedEnumTypeName = GetReferencedEnumTypeName(propertySchema);
            if (referencedEnumTypeName != null)
            {
                AddPropertyInfo(
                    entries,
                    propertyName,
                    ComparisonKind.OperatorEquals,
                    HashKind.ScalarValueType,
                    InitializationKind.SimpleAssign,
                    MakeNamedType(referencedEnumTypeName));
            }

            EnumHint enumHint;
            if (propertySchema.ShouldBeEnum(_typeName, propertyName, _hintDictionary, out enumHint))
            {
                AddPropertyInfo(
                    entries,
                    propertyName,
                    ComparisonKind.OperatorEquals,
                    HashKind.ScalarValueType,
                    InitializationKind.SimpleAssign,
                    MakeNamedType(enumHint.TypeName));
            }

            switch (propertySchema.Type)
            {
                case JsonType.Boolean:
                case JsonType.Integer:
                case JsonType.Number:
                    AddPropertyInfo(
                        entries,
                        propertyName,
                            ComparisonKind.OperatorEquals,
                        HashKind.ScalarValueType,
                        InitializationKind.SimpleAssign,
                        MakePrimitiveType(propertySchema.Type));
                    break;

                case JsonType.String:
                    AddPropertyInfo(
                        entries,
                        propertyName,
                            ComparisonKind.OperatorEquals,
                        HashKind.ScalarReferenceType,
                        InitializationKind.SimpleAssign,
                        MakePrimitiveType(propertySchema.Type));
                    break;

                case JsonType.Object:
                    // If the schema for this property references a named type,
                    // the generated Init method will initialize it by cloning an object
                    // of that type. Otherwise, we treat this property as a System.Object
                    // and just initialize it by assignment.
                    InitializationKind initializationKind = propertySchema.Reference != null
                        ? InitializationKind.Clone
                        : InitializationKind.SimpleAssign;

                    AddPropertyInfo(
                        entries,
                        propertyName,
                            ComparisonKind.ObjectEquals,
                        HashKind.ScalarReferenceType,
                        initializationKind,
                        MakeObjectType(propertySchema));
                    break;

                case JsonType.Array:
                    AddPropertyInfo(
                        entries,
                        propertyName,
                            ComparisonKind.Collection,
                        HashKind.Collection,
                        InitializationKind.Collection,
                        MakeArrayType(entries, propertyName, propertySchema));
                    break;

                case JsonType.None:
                    JsonType inferredType = InferJsonTypeFromEnumValues(propertySchema.Enum);
                    if (inferredType == JsonType.None)
                    {
                        AddPropertyInfo(
                            entries,
                            propertyName,
                                    ComparisonKind.ObjectEquals,
                            HashKind.ScalarReferenceType,
                            InitializationKind.None,
                            MakePrimitiveType(JsonType.Object));
                        break;

                    }
                    else if (inferredType == JsonType.String)
                    {
                        AddPropertyInfo(
                            entries,
                            propertyName,
                                    ComparisonKind.OperatorEquals,
                            HashKind.ScalarReferenceType,
                            InitializationKind.SimpleAssign,
                            MakePrimitiveType(JsonType.String));
                        break;
                    }
                    else
                    {
                        AddPropertyInfo(
                            entries,
                            propertyName,
                                    ComparisonKind.OperatorEquals,
                            HashKind.ScalarValueType,
                            InitializationKind.SimpleAssign,
                            MakePrimitiveType(inferredType));
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(propertySchema.Type));
            }
        }

        private void AddPropertyInfo(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string key,
            ComparisonKind comparisonKind,
            HashKind hashKind,
            InitializationKind initializationKind,
            TypeSyntax type)
        {
            entries.Add(new KeyValuePair<string, PropertyInfo>(
                key,
                new PropertyInfo(
                    comparisonKind,
                    hashKind,
                    initializationKind,
                    type)));
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

        private static TypeSyntax MakeNamedType(string typeName)
        {
            int index = typeName.LastIndexOf('.');
            if (index != -1)
            {
                typeName = typeName.Substring(index + 1);
            }

            typeName = typeName.ToPascalCase();

            return SyntaxFactory.ParseTypeName(typeName);
        }

        private TypeSyntax MakeArrayType(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string propertyName,
            JsonSchema schema)
        {
            string key = MakeElementKeyName(propertyName);
            AddPropertyInfoFromPropertySchema(entries, key, schema.Items);
            PropertyInfo info = entries.Single(kvp => kvp.Key == key).Value;

            // Create a list of whatever this property is. If the property
            // is itself an array, this will result in a list of lists, and so on.
            return SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("IList"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                        new TypeSyntax[]
                        {
                            info.Type
                        })));
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
            return _hintDictionary != null
                && _hintDictionary.Any(
                    kvp => kvp.Key.Equals(definitionName)
                    && kvp.Value.Any(hint => hint is EnumHint));
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
    }
}
