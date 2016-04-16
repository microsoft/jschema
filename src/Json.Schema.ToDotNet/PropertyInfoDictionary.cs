// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

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

        private static readonly Dictionary<JTokenType, SyntaxKind> s_JTokenTypeToSyntaxKindDictionary = new Dictionary<JTokenType, SyntaxKind>
        {
            [JTokenType.Boolean] = SyntaxKind.BoolKeyword,
            [JTokenType.Integer] = SyntaxKind.IntKeyword,
            [JTokenType.Float] = SyntaxKind.DoubleKeyword,
            [JTokenType.String] = SyntaxKind.StringKeyword
        };

        // A string which, when appended to a property name used as a key into the
        // PropertyInfoDictionary, indicates that the property is an array. For
        // example, <code>Location[]</code> is an array property, and
        // <code>Location[][]</code> is a property that is an array of arrays.
        internal const string ArrayMarker = "[]";

        // A string which, when appended to a property name used as a key into the
        // PropertyInfoDictionary, indicates that the property is an dictionary. For
        // example, <code>Location{}</code> is an dictionary property, and
        // <code>Location{}[]</code> is a dictionary property whose elements are arrays.
        internal const string DictionaryMarker = "{}";

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
            return propertyName.ToPascalCase() + ArrayMarker;
        }

        /// <summary>
        /// Synthesize a lookup key by which the items of the specified dictionary-
        /// valued property can be looked up in the <see cref="PropertyInfoDictionary"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The name of a dictionary-valued property.
        /// </param>
        /// <returns>
        /// A lookup key for the items of the property specified by <paramref name="propertyName"/>.
        /// </returns>
        public static string MakeDictionaryItemKeyName(string propertyName)
        {
            return propertyName.ToPascalCase() + DictionaryMarker;
        }

        public static SyntaxKind GetTypeKeywordFromJTokenType(JTokenType type)
        {
            SyntaxKind typeKeyword;
            if (!s_JTokenTypeToSyntaxKindDictionary.TryGetValue(type, out typeKeyword))
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
            bool isOfSchemaDefinedType = false;
            int arrayRank = 0;
            EnumHint enumHint;
            string dictionaryKeyTypeName;

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
            else if (propertySchema.ShouldBeDictionary(_typeName, propertyName, _hintDictionary, out dictionaryKeyTypeName))
            {
                comparisonKind = ComparisonKind.Dictionary;
                hashKind = HashKind.Dictionary;
                initializationKind = InitializationKind.Dictionary;

                // If the schema for this property specifies additionalProperties, and if
                // the value of additionalProperties is a schema as opposed to a Boolean,
                // then we will represent this property as a dictionary from string to
                // whatever kind of object the schema represents. Otherwise, treat it as
                // a dictionary from string to string.
                JsonSchema dictionaryElementSchema = propertySchema.AdditionalProperties?.Schema != null
                    ? propertySchema.AdditionalProperties.Schema
                    : new JsonSchema { Type = new JTokenType[] { JTokenType.String } };

                type = MakeDictionaryType(entries, propertyName, dictionaryKeyTypeName, dictionaryElementSchema);
                namespaceName = "System.Collections.Generic";   // For IDictionary.
            }
            else if ((referencedEnumTypeName = GetReferencedEnumTypeName(propertySchema)) != null)
            {
                comparisonKind = ComparisonKind.OperatorEquals;
                hashKind = HashKind.ScalarValueType;
                initializationKind = InitializationKind.SimpleAssign;
                type = MakeNamedType(referencedEnumTypeName, out namespaceName);
            }
            else if (propertySchema.ShouldBeEnum(_typeName, propertyName,  _hintDictionary, out enumHint))
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
                JTokenType propertyType = propertySchema.Type == null || propertySchema.Type.Length == 0
                    ? JTokenType.None
                    : propertySchema.Type[0];

                switch (propertyType)
                {
                    case JTokenType.Boolean:
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        comparisonKind = ComparisonKind.OperatorEquals;
                        hashKind = HashKind.ScalarValueType;
                        initializationKind = InitializationKind.SimpleAssign;
                        type = MakePrimitiveType(propertyType);
                        break;

                    case JTokenType.String:
                        comparisonKind = ComparisonKind.OperatorEquals;
                        hashKind = HashKind.ScalarReferenceType;
                        initializationKind = InitializationKind.SimpleAssign;
                        type = MakePrimitiveType(propertyType);
                        break;

                    case JTokenType.Object:
                        // If the schema for this property references a named type,
                        // the generated Init method will initialize it by cloning an object
                        // of that type. Otherwise, we treat this property as a System.Object
                        // and just initialize it by assignment.
                        if (propertySchema.Reference != null)
                        {
                            initializationKind = InitializationKind.Clone;
                            isOfSchemaDefinedType = true;
                        }
                        else
                        {
                            initializationKind = InitializationKind.SimpleAssign;
                        }

                        comparisonKind = ComparisonKind.ObjectEquals;
                        hashKind = HashKind.ScalarReferenceType;
                        type = MakeObjectType(propertySchema, out namespaceName);
                        break;

                    case JTokenType.Array:
                        comparisonKind = ComparisonKind.Collection;
                        hashKind = HashKind.Collection;
                        initializationKind = InitializationKind.Collection;
                        namespaceName = "System.Collections.Generic";   // For IList.
                        type = MakeArrayType(entries, propertyName, propertySchema);
                        break;

                    case JTokenType.None:
                        JTokenType inferredType = InferJTokenTypeFromEnumValues(propertySchema.Enum);
                        if (inferredType == JTokenType.None)
                        {
                            comparisonKind = ComparisonKind.ObjectEquals;
                            hashKind = HashKind.ScalarReferenceType;
                            initializationKind = InitializationKind.None;
                            type = MakePrimitiveType(JTokenType.Object);
                            break;

                        }
                        else if (inferredType == JTokenType.String)
                        {
                            comparisonKind = ComparisonKind.OperatorEquals;
                            hashKind = HashKind.ScalarReferenceType;
                            initializationKind = InitializationKind.SimpleAssign;
                            type = MakePrimitiveType(JTokenType.String);
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

            entries.Add(new KeyValuePair<string, PropertyInfo>(
                propertyName.ToPascalCase(),
                new PropertyInfo(
                    propertySchema.Description,
                    comparisonKind,
                    hashKind,
                    initializationKind,
                    type,
                    namespaceName,
                    isRequired,
                    isOfSchemaDefinedType,
                    arrayRank,
                    entries.Count)));
        }

        private TypeSyntax MakePrimitiveType(JTokenType JTokenType)
        {
            SyntaxKind typeKeyword = GetTypeKeywordFromJTokenType(JTokenType);
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

        private TypeSyntax MakeDictionaryType(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string propertyName,
            string keyTypeName,
            JsonSchema dictionaryElementSchema)
        {
            string key = MakeDictionaryItemKeyName(propertyName);
            AddPropertyInfoFromPropertySchema(entries, key, dictionaryElementSchema, false);
            PropertyInfo info = entries.Single(kvp => kvp.Key == key).Value;


            // Create a dictionary of whatever this property is. If the property
            // is an array, this will result in a dictionary of lists, and so on.
            return SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("IDictionary"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        new TypeSyntax[]
                        {
                            SyntaxFactory.ParseTypeName(keyTypeName),
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

        private JTokenType InferJTokenTypeFromEnumValues(object[] enumValues)
        {
            JTokenType JTokenType = JTokenType.None;

            if (enumValues != null && enumValues.Any())
            {
                JTokenType = GetJTokenTypeFromObject(enumValues[0]);
                for (int i = 1; i < enumValues.Length; ++i)
                {
                    if (GetJTokenTypeFromObject(enumValues[i]) != JTokenType)
                    {
                        JTokenType = JTokenType.None;
                        break;
                    }
                }
            }

            return JTokenType;
        }

        private static JTokenType GetJTokenTypeFromObject(object obj)
        {
            if (obj is string)
            {
                return JTokenType.String;
            }
            else if (obj.IsIntegralType())
            {
                return JTokenType.Integer;
            }
            else if (obj.IsFloatingType())
            {
                return JTokenType.Float;
            }
            else if (obj is bool)
            {
                return JTokenType.Boolean;
            }
            else
            {
                return JTokenType.None;
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
