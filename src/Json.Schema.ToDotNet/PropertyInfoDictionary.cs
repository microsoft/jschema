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
    public class PropertyInfoDictionary : IReadOnlyDictionary<string, PropertyInfo>
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
        /// Callback invoked when the dictionary discovers that another type must be
        /// generated, in addition to the one whose properties it is already generating.
        /// </summary>
        /// <remarks>
        /// For example, when the dictionary encounters a property for which an
        /// <see cref="EnumHint"/> is specified, it raises this event to signal that
        /// an enumerated type must also be generated.
        /// </remarks>
        public delegate void AdditionalTypeRequiredDelegate(AdditionalTypeRequiredInfo additionalTypeRequiredInfo);

        private AdditionalTypeRequiredDelegate _additionalTypeRequiredDelegate;

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
        public PropertyInfoDictionary(
            string typeName,
            JsonSchema schema,
            HintDictionary hintDictionary,
            AdditionalTypeRequiredDelegate additionalTypeRequiredDelegate)
        {
            _typeName = typeName;
            _schema = schema;
            _hintDictionary = hintDictionary;
            _additionalTypeRequiredDelegate = additionalTypeRequiredDelegate;

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
                    bool isRequired = _schema.Required?.Contains(propertyName) == true;

                    AddPropertyInfoFromPropertySchema(entries, propertyName, subSchema, isRequired);
                }
            }

            return ImmutableDictionary.CreateRange(entries);
        }

        private void AddPropertyInfoFromPropertySchema(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string propertyName,
            JsonSchema propertySchema,
            bool isRequired)
        {
            ComparisonKind comparisonKind;
            HashKind hashKind;
            InitializationKind initializationKind;
            TypeSyntax type;
            string namespaceName = null;
            string referencedEnumTypeName;
            EnumHint enumHint;

            if (propertySchema.IsDateTime())
            {
                comparisonKind = ComparisonKind.OperatorEquals;
                hashKind = HashKind.ScalarValueType;
                initializationKind = InitializationKind.SimpleAssign;
                type = MakeNamedType("System.DateTime", out namespaceName);
            }
            else if (propertySchema.IsUri())
            {
                comparisonKind = ComparisonKind.OperatorEquals;
                hashKind = HashKind.ScalarReferenceType;
                initializationKind = InitializationKind.Uri;
                type = MakeNamedType("System.Uri", out namespaceName);
            }
            else if (propertySchema.ShouldBeDictionary(_typeName, propertyName, _hintDictionary))
            {
                comparisonKind = ComparisonKind.Dictionary;
                hashKind = HashKind.Dictionary;
                initializationKind = InitializationKind.Clone;
                type = MakeNamedType("System.Collections.Generic.Dictionary<string, string>", out namespaceName);
            }
            else if ((referencedEnumTypeName = GetReferencedEnumTypeName(propertySchema)) != null)
            {
                comparisonKind = ComparisonKind.OperatorEquals;
                hashKind = HashKind.ScalarValueType;
                initializationKind = InitializationKind.SimpleAssign;
                type = MakeNamedType(referencedEnumTypeName, out namespaceName);
            }
            else if (propertySchema.ShouldBeEnum(_typeName, propertyName, _hintDictionary, out enumHint))
            {
                comparisonKind = ComparisonKind.OperatorEquals;
                hashKind = HashKind.ScalarValueType;
                initializationKind = InitializationKind.SimpleAssign;
                type = MakeNamedType(enumHint.TypeName, out namespaceName);

                // The class whose property info we are generating contains a property
                // of an enumerated type. Notify the code generator that it must generate
                // the enumerated type in addition to the current type.
                OnAdditionalTypeRequired(enumHint, propertySchema);
            }
            else
        	{
                switch (propertySchema.Type)
                {
                    case JsonType.Boolean:
                    case JsonType.Integer:
                    case JsonType.Number:
                        comparisonKind = ComparisonKind.OperatorEquals;
                        hashKind = HashKind.ScalarValueType;
                        initializationKind = InitializationKind.SimpleAssign;
                        type = MakePrimitiveType(propertySchema.Type);
                        break;

                    case JsonType.String:
                        comparisonKind = ComparisonKind.OperatorEquals;
                        hashKind = HashKind.ScalarReferenceType;
                        initializationKind = InitializationKind.SimpleAssign;
                        type = MakePrimitiveType(propertySchema.Type);
                        break;

                    case JsonType.Object:
                        // If the schema for this property references a named type,
                        // the generated Init method will initialize it by cloning an object
                        // of that type. Otherwise, we treat this property as a System.Object
                        // and just initialize it by assignment.
                        initializationKind = propertySchema.Reference != null
                            ? InitializationKind.Clone
                            : InitializationKind.SimpleAssign;

                        comparisonKind = ComparisonKind.ObjectEquals;
                        hashKind = HashKind.ScalarReferenceType;
                        type = MakeObjectType(propertySchema, out namespaceName);
                        break;

                    case JsonType.Array:
                        comparisonKind = ComparisonKind.Collection;
                        hashKind = HashKind.Collection;
                        initializationKind = InitializationKind.Collection;
                        namespaceName = "System.Collections.Generic";   // For IList.
                        type = MakeArrayType(entries, propertyName, propertySchema);
                        break;

                    case JsonType.None:
                        JsonType inferredType = InferJsonTypeFromEnumValues(propertySchema.Enum);
                        if (inferredType == JsonType.None)
                        {
                            comparisonKind = ComparisonKind.ObjectEquals;
                            hashKind = HashKind.ScalarReferenceType;
                            initializationKind = InitializationKind.None;
                            type = MakePrimitiveType(JsonType.Object);
                            break;

                        }
                        else if (inferredType == JsonType.String)
                        {
                            comparisonKind = ComparisonKind.OperatorEquals;
                            hashKind = HashKind.ScalarReferenceType;
                            initializationKind = InitializationKind.SimpleAssign;
                            type = MakePrimitiveType(JsonType.String);
                            break;
                        }
                        else
                        {
                            comparisonKind = ComparisonKind.OperatorEquals;
                            hashKind = HashKind.ScalarValueType;
                            initializationKind = InitializationKind.SimpleAssign;
                            type = MakePrimitiveType(inferredType);
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(propertySchema.Type));
                }
            }

            AddPropertyInfo(
                entries,
                propertyName,
                propertySchema.Description,
                comparisonKind,
                hashKind,
                initializationKind,
                type,
                namespaceName,
                isRequired);
        }

        private void AddPropertyInfo(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string key,
            string description,
            ComparisonKind comparisonKind,
            HashKind hashKind,
            InitializationKind initializationKind,
            TypeSyntax type,
            string namespaceName,
            bool isRequired)
        {
            entries.Add(new KeyValuePair<string, PropertyInfo>(
                key,
                new PropertyInfo(
                    description,
                    comparisonKind,
                    hashKind,
                    initializationKind,
                    type,
                    namespaceName,
                    isRequired,
                    entries.Count)));
        }

        private TypeSyntax MakePrimitiveType(JsonType jsonType)
        {
            SyntaxKind typeKeyword = GetTypeKeywordFromJsonType(jsonType);
            return SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword));
        }

        private TypeSyntax MakeObjectType(JsonSchema schema, out string namespaceName)
        {
            namespaceName = null;

            if (schema.Reference != null)
            {
                return MakeNamedType(schema.Reference.GetDefinitionName(), out namespaceName);
            }
            else
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
            }
        }

        private TypeSyntax MakeNamedType(string typeName, out string namespaceName)
        {
            string unqualifiedTypeName = GetUnqualifiedTypeName(typeName, out namespaceName);

            return SyntaxFactory.ParseTypeName(unqualifiedTypeName);
        }

        private TypeSyntax MakeArrayType(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string propertyName,
            JsonSchema schema)
        {
            string key = MakeElementKeyName(propertyName);
            AddPropertyInfoFromPropertySchema(entries, key, schema.Items, false);
            PropertyInfo info = entries.Single(kvp => kvp.Key == key).Value;

            // Create a list of whatever this property is. If the property
            // is itself an array, this will result in a list of lists, and so on.
            return SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("IList"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(info.Type)));
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

        private static string GetUnqualifiedTypeName(string typeName, out string namespaceName)
        {
            string unqualifiedTypeName;

            int index = typeName.LastIndexOf('.');
            if (index != -1)
            {
                unqualifiedTypeName = typeName.Substring(index + 1);
                namespaceName = typeName.Substring(0, index);
            }
            else
            {
                unqualifiedTypeName = typeName;
                namespaceName = null;
            }

            return unqualifiedTypeName.ToPascalCase();
        }

        private void OnAdditionalTypeRequired(CodeGenHint hint, JsonSchema schema)
        {
            _additionalTypeRequiredDelegate?.Invoke(
                new AdditionalTypeRequiredInfo(hint, schema));
        }
    }
}
