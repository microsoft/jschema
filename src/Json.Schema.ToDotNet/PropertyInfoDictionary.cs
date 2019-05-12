// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Json.Schema.ToDotNet.Hints;

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

        private static readonly Dictionary<SchemaType, SyntaxKind> s_SchemaTypeToSyntaxKindDictionary = new Dictionary<SchemaType, SyntaxKind>
        {
            [SchemaType.Boolean] = SyntaxKind.BoolKeyword,
            [SchemaType.Integer] = SyntaxKind.IntKeyword,
            [SchemaType.Number] = SyntaxKind.DoubleKeyword,
            [SchemaType.String] = SyntaxKind.StringKeyword
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

        private readonly AdditionalTypeRequiredDelegate _additionalTypeRequiredDelegate;
        private readonly string _typeNameSuffix;

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
            string typeNameSuffix,
            JsonSchema schema,
            HintDictionary hintDictionary,
            AdditionalTypeRequiredDelegate additionalTypeRequiredDelegate)
        {
            _typeName = typeName;
            _typeNameSuffix = typeNameSuffix;
            _schema = schema;
            _hintDictionary = hintDictionary;
            _additionalTypeRequiredDelegate = additionalTypeRequiredDelegate;

            _dictionary = PropertyInfoDictionaryFromSchema();
        }

        /// <summary>
        /// Gets the list of all properties declared in the schema.
        /// </summary>
        /// <remarks>
        /// Don't include information about array elements or dictionary entries.
        /// For example, if the class has an array-valued property ArrayProp, then
        /// include "ArrayProp" in the list, but not "ArrayProp[]". Similarly, if the
        /// class has a dictionary-valued property DictProp, then include "DictProp" in
        /// the list, but not "DictProp{}".
        /// </remarks>
        /// <returns>
        /// An array containing the names of the properties.
        /// </returns>
        public string[] GetPropertyNames()
        {
            return this.Keys
                .Where(key => key.IndexOf(ArrayMarker) == -1
                                && key.IndexOf(DictionaryMarker) == -1)
                .OrderBy(key => this[key].DeclarationOrder)
                .Select(key => key.ToPascalCase())
                .ToArray();
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

        public static SyntaxKind GetTypeKeywordFromSchemaType(SchemaType type)
        {
            if (!s_SchemaTypeToSyntaxKindDictionary.TryGetValue(type, out SyntaxKind typeKeyword))
            {
                typeKeyword = SyntaxKind.ObjectKeyword;
            }

            return typeKeyword;
        }

        #region IReadOnlyDictionary

        public PropertyInfo this[string key]
        {
            get
            {
                if (!TryGetValue(key, out PropertyInfo info))
                {
                    throw new ApplicationException($"The schema does not contain information describing the property or element {key}.");
                }

                return info;
            }
        }

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
                    JsonSchema propertySchema = schemaProperty.Value;
                    bool isRequired = _schema.Required?.Contains(propertyName) == true;

                    AddPropertyInfoFromPropertySchema(entries, propertyName, propertySchema, isRequired);
                }
            }

            return ImmutableDictionary.CreateRange(entries);
        }

        private void AddPropertyInfoFromPropertySchema(
            IList<KeyValuePair<string, PropertyInfo>> entries,
            string schemaPropertyName,
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
            else if (propertySchema.ShouldBeDictionary(_typeName, schemaPropertyName, _hintDictionary, out DictionaryHint dictionaryHint))
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
                    : new JsonSchema { Type = new SchemaType[] { SchemaType.String } };

                type = MakeDictionaryType(entries, schemaPropertyName, dictionaryHint, dictionaryElementSchema);
                namespaceName = "System.Collections.Generic";   // For IDictionary.
            }
            else if ((referencedEnumTypeName = GetReferencedEnumTypeName(propertySchema)) != null)
            {
                comparisonKind = ComparisonKind.OperatorEquals;
                hashKind = HashKind.ScalarValueType;
                initializationKind = InitializationKind.SimpleAssign;
                type = MakeNamedType(referencedEnumTypeName, out namespaceName);
            }
            else if (propertySchema.ShouldBeEnum(_typeName, schemaPropertyName,  _hintDictionary, out EnumHint enumHint))
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
                SchemaType propertyType = propertySchema.SafeGetType();

                switch (propertyType)
                {
                    case SchemaType.Boolean:
                    case SchemaType.Integer:
                    case SchemaType.Number:
                        comparisonKind = ComparisonKind.OperatorEquals;
                        hashKind = HashKind.ScalarValueType;
                        initializationKind = InitializationKind.SimpleAssign;
                        type = MakePrimitiveType(propertyType);
                        break;

                    case SchemaType.String:
                        comparisonKind = ComparisonKind.OperatorEquals;
                        hashKind = HashKind.ScalarReferenceType;
                        initializationKind = InitializationKind.SimpleAssign;
                        type = MakePrimitiveType(propertyType);
                        break;

                    case SchemaType.Object:
                        // If the schema for this property references a named type,
                        // the generated Init method will initialize it by cloning an object
                        // of that type. Otherwise, we treat this property as a System.Object
                        // and just initialize it by assignment.
                        if (propertySchema.Reference != null)
                        {
                            comparisonKind = ComparisonKind.EqualityComparerEquals;
                            initializationKind = InitializationKind.Clone;
                            hashKind = HashKind.ObjectModelType;
                            isOfSchemaDefinedType = true;
                        }
                        else
                        {
                            comparisonKind = ComparisonKind.ObjectEquals;
                            initializationKind = InitializationKind.SimpleAssign;
                            hashKind = HashKind.ScalarReferenceType;
                        }

                        type = MakeObjectType(propertySchema, out namespaceName);
                        break;

                    case SchemaType.Array:
                        comparisonKind = ComparisonKind.Collection;
                        hashKind = HashKind.Collection;
                        initializationKind = InitializationKind.Collection;
                        namespaceName = "System.Collections.Generic";   // For IList.
                        type = MakeArrayType(entries, schemaPropertyName, propertySchema);
                        break;

                    case SchemaType.None:
                        SchemaType inferredType = InferSchemaTypeFromEnumValues(propertySchema.Enum);
                        if (inferredType == SchemaType.None)
                        {
                            comparisonKind = ComparisonKind.ObjectEquals;
                            hashKind = HashKind.ScalarReferenceType;
                            initializationKind = InitializationKind.None;
                            type = MakePrimitiveType(SchemaType.Object);
                            break;

                        }
                        else if (inferredType == SchemaType.String)
                        {
                            comparisonKind = ComparisonKind.OperatorEquals;
                            hashKind = HashKind.ScalarReferenceType;
                            initializationKind = InitializationKind.SimpleAssign;
                            type = MakePrimitiveType(SchemaType.String);
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

            var propertyNameHint = _hintDictionary?.GetHint<PropertyNameHint>(_typeName + "." + schemaPropertyName);
            string dotNetPropertyName = propertyNameHint != null
                ? propertyNameHint.DotNetPropertyName
                : schemaPropertyName.ToPascalCase();

            entries.Add(new KeyValuePair<string, PropertyInfo>(
                dotNetPropertyName,
                new PropertyInfo(
                    propertySchema.Description,
                    schemaPropertyName,
                    comparisonKind,
                    hashKind,
                    initializationKind,
                    type,
                    namespaceName,
                    isRequired,
                    propertySchema.Default,
                    isOfSchemaDefinedType,
                    arrayRank,
                    entries.Count)));
        }

        private TypeSyntax MakePrimitiveType(SchemaType schemaType)
        {
            SyntaxKind typeKeyword = GetTypeKeywordFromSchemaType(schemaType);
            return SyntaxFactory.PredefinedType(SyntaxFactory.Token(typeKeyword));
        }

        private TypeSyntax MakeObjectType(JsonSchema schema, out string namespaceName)
        {
            namespaceName = null;

            if (schema.Reference != null)
            {
                return MakeObjectTypeFromReference(schema.Reference, out namespaceName);
            }
            else
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
            }
        }

        private TypeSyntax MakeObjectTypeFromReference(UriOrFragment reference, out string namespaceName)
        {
            string className = reference.GetDefinitionName();
            ClassNameHint classNameHint = _hintDictionary?.GetHint<ClassNameHint>(className.ToCamelCase());
            if (classNameHint != null)
            {
                className = classNameHint.ClassName;
            }

            return MakeNamedType(className, out namespaceName);
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
            if (schema.Items == null)
            {
                // If "items" is missing, it defaults to an empty schema, meaning the array
                // element type can be anything. By treating it as if it were "items": "object",
                // we will generate the same code, namely, a .NET array of System.Object.
                schema.Items = new Items(
                    new JsonSchema
                    {
                        Type = new List<SchemaType>
                        {
                            SchemaType.Object
                        }
                    });
            }

            if (!schema.Items.SingleSchema)
            {
                throw new ApplicationException($"Cannot generate code for the array property '{propertyName}' because the 'items' property of the schema contains different schemas for each array element.");
            }

            AddPropertyInfoFromPropertySchema(entries, key, schema.Items.Schema, isRequired: true);
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
            DictionaryHint dictionaryHint,
            JsonSchema dictionaryElementSchema)
        {
            string key = MakeDictionaryItemKeyName(propertyName);

            string keyTypeName = dictionaryHint?.KeyTypeName ?? WellKnownTypeNames.String;

            // An explicitly hinted value type takes precedence over what's declared in
            // the schema.
            TypeSyntax valueType;
            if (dictionaryHint?.ValueTypeName != null)
            {
                valueType = SyntaxFactory.ParseTypeName(dictionaryHint.ValueTypeName);
                entries.Add(new KeyValuePair<string, PropertyInfo>(
                    key,
                    new PropertyInfo(
                        description: string.Empty,
                        serializedName: null,
                        comparisonKind: dictionaryHint.ComparisonKind,
                        hashKind: dictionaryHint.HashKind,
                        initializationKind: dictionaryHint.InitializationKind,
                        type: valueType,
                        namespaceName: dictionaryHint.NamespaceName,
                        isRequired: true,
                        defaultValue: null,
                        isOfSchemaDefinedType: false,
                        arrayRank: 0,
                        declarationOrder: 0)));
            }
            else
            {
                AddPropertyInfoFromPropertySchema(entries, key, dictionaryElementSchema, isRequired: true);
                PropertyInfo info = entries.Single(kvp => kvp.Key == key).Value;
                valueType = info.Type;
            }

            // Create a dictionary of whatever this property is. If the property
            // is an array, this will result in a dictionary of lists, and so on.
            return SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("IDictionary"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        new TypeSyntax[]
                        {
                            SyntaxFactory.ParseTypeName(keyTypeName),
                            valueType
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

        private SchemaType InferSchemaTypeFromEnumValues(IList<object> enumValues)
        {
            SchemaType schemaType = SchemaType.None;

            if (enumValues != null && enumValues.Any())
            {
                schemaType = GetSchemaTypeFromObject(enumValues[0]);
                for (int i = 1; i < enumValues.Count; ++i)
                {
                    if (GetSchemaTypeFromObject(enumValues[i]) != schemaType)
                    {
                        schemaType = SchemaType.None;
                        break;
                    }
                }
            }

            return schemaType;
        }

        private static SchemaType GetSchemaTypeFromObject(object obj)
        {
            if (obj is string)
            {
                return SchemaType.String;
            }
            else if (obj.IsIntegralType())
            {
                return SchemaType.Integer;
            }
            else if (obj.IsFloatingType())
            {
                return SchemaType.Number;
            }
            else if (obj is bool)
            {
                return SchemaType.Boolean;
            }
            else
            {
                return SchemaType.None;
            }
        }

        private string GetUnqualifiedTypeName(string typeName, out string namespaceName)
        {
            string unqualifiedTypeName;

            int index = typeName.LastIndexOf('.');
            if (index != -1)
            {
                // We have a namespaced .NET type
                unqualifiedTypeName = typeName.Substring(index + 1);
                namespaceName = typeName.Substring(0, index);
            }
            else
            {
                // One of our schema types, add the type name suffix, if specified
                unqualifiedTypeName = typeName + _typeNameSuffix;
                namespaceName = null;
            }

            return unqualifiedTypeName.ToPascalCase();
        }

        private void OnAdditionalTypeRequired(CodeGenHint hint, JsonSchema schema)
        {
            _additionalTypeRequiredDelegate?.Invoke(
                new AdditionalTypeRequiredInfo(hint, schema));
        }

        /// <summary>
        /// Synthesize the concrete type that should be used to initialize a collection-
        /// valued property in the implementation of the generated class's <code>Init</code>
        /// method.
        /// </summary>
        /// <remarks>
        /// For array-valued properties, the property type stored in the
        /// PropertyInfoDictionary is <see cref="IList{T}" />. But in the implementation
        /// of the <code>Init</code> method, the concrete type used to initialize the
        /// property is <see cref="List{T}" />.
        /// </remarks>
        internal TypeSyntax GetConcreteListType(string propertyName)
        {
            TypeSyntax type = this[propertyName].Type;

            string typeName = type.ToString();
            if (typeName.StartsWith("IList"))
            {
                typeName = typeName.Substring(1);
                type = SyntaxFactory.ParseTypeName(typeName);
            }

            return type;
        }

        /// <summary>
        /// Synthesize the concrete type that should be used to initialize a dictionary-
        /// valued property in the implementation of the generated class's <code>Init</code>
        /// method.
        /// <remarks>
        /// For dictionary-valued properties, the property type stored in the
        /// PropertyInfoDictionary is <see cref="IDictionary{K,V}" />. But in the
        /// implementation of the <code>Init</code> method, the concrete type used to
        /// initialize the property is <see cref="Dictionary{K,V}" />.
        /// </remarks>
        internal TypeSyntax GetConcreteDictionaryType(string propertyName)
        {
            TypeSyntax type = this[propertyName].Type;

            string typeName = type.ToString();
            if (typeName.StartsWith("IDictionary"))
            {
                typeName = typeName.Substring(1);
                type = SyntaxFactory.ParseTypeName(typeName);
            }

            return type;
        }
    }
}
