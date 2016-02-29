// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// TODO: Setting _jsonType or _className should set InferredTypeKind.

namespace Microsoft.JSchema
{
    /// <summary>
    /// Represents the data type of a property as inferred from the JSON schema.
    /// </summary>
    /// <remarks>
    /// A schema can specify a type, which must be one of the "primitive" types defined
    /// by the JSON Schema specification. If a schema does not specify a type, it is
    /// sometimes possible to infer the type. The inferred type might be one of the
    /// primitive types, or it might be a class type whose name can be inferred. This
    /// class represents the inferred type, whichever of those two forms was inferred.
    /// </remarks>
    public class InferredType : IEquatable<InferredType>
    {
        /// <summary>
        /// An instance of the <see cref="InferredType"/> class that does not represent
        /// any inferred type.
        /// </summary>
        public static readonly InferredType None = new InferredType(JsonType.None);

        private readonly JsonSchema _rootSchema;
        private JsonType _jsonType;
        private string _className;

        /// <summary>
        /// Initializes a new instance of the <see cref="InferredType"/> class from a 
        /// subschema of a JSON schema.
        /// </summary>
        /// <param name="rootSchema">
        /// The root JSON schema which contains, directly or indirectly, the subschema
        /// from which the type is to be inferred.
        /// </param>
        /// <param name="subSchema">
        /// The JSON schema from which the type is to be inferred.
        /// </param>
        public InferredType(JsonSchema rootSchema, JsonSchema subSchema)
        {
            _rootSchema = rootSchema;
            InferTypeFromSchema(subSchema);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InferredType"/> class from a JSON schema.
        /// </summary>
        /// <param name="rootSchema">
        /// The JSON schema from which the type is to be inferred.
        /// </param>
        public InferredType(JsonSchema rootSchema)
            : this(rootSchema, rootSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InferredType"/> class from one of
        /// the primitive types defined by JSON Schema.
        /// </summary>
        /// <param name="jsonType">
        /// One of the primitive types defined by JSON Schema.
        /// </param>
        /// <remarks>
        /// The <see cref="Kind"/> property of the resulting instance is <see cref="InferredTypeKind.JsonType"/>.
        /// </remarks>
        public InferredType(JsonType jsonType)
        {
            _jsonType = jsonType;
            Kind = InferredTypeKind.JsonType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InferredType"/> class from a class name.
        /// </summary>
        /// <param name="className">
        /// The name of a class.
        /// </param>
        /// <remarks>
        /// The <see cref="Kind"/> property of the resulting instance is <see cref="InferredTypeKind.ClassName"/>.
        /// </remarks>
        public InferredType(string className)
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            _className = className;
            Kind = InferredTypeKind.ClassName;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InferredType"/> that does not represent any inferred
        /// type.
        /// </summary>
        private InferredType()
        {
            Kind = InferredTypeKind.None;
        }

        /// <summary>
        /// Gets a value specifying whether the inferred type is a JSON primitive type or
        /// a class name.
        /// </summary>
        public InferredTypeKind Kind { get; private set; }

        /// <summary>
        /// Returns the inferred JSON primitive type, if <see cref="Kind"/> property of this
        /// instance is <see cref="InferredTypeKind.JsonType"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If <see cref="Kind"/> property of this instance is not <see cref="InferredTypeKind.JsonType"/>.
        /// </exception>
        public JsonType GetJsonType()
        {
            if (Kind != InferredTypeKind.JsonType)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                        Resources.ErrorNoPrimitiveTypeForInferredTypeKindClassName,
                        nameof(Kind),
                        nameof(InferredType),
                        nameof(InferredTypeKind.JsonType)));
            }

            return _jsonType;
        }

        /// <summary>
        /// Returns the inferred class name, if <see cref="Kind"/> property of this instance
        /// is <see cref="InferredTypeKind.ClassName"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If <see cref="Kind"/> property of this instance is not <see cref="InferredTypeKind.ClassName"/>.
        /// </exception>
        public string GetClassName()
        {
            if (Kind != InferredTypeKind.ClassName)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                        Resources.ErrorNoClassNameForInferredTypeKindPrimitive,
                        nameof(Kind),
                        nameof(InferredType),
                        nameof(InferredTypeKind.JsonType)));
            }

            return _className;
        }

        #region Object

        public override bool Equals(object other)
        {
            return Equals(other as InferredType);
        }

        #endregion Object

        #region IEquatable<T>

        public bool Equals(InferredType other)
        {
            if ((object)other == null || Kind != other.Kind)
            {
                return false;
            }

            if (Kind == InferredTypeKind.JsonType)
            {
                return _jsonType == other._jsonType;
            }

            return _className.Equals(_className, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            int dataHash = Kind == InferredTypeKind.JsonType
                ? _jsonType.GetHashCode()
                : _className.GetHashCode();

            return Hash.Combine(Kind.GetHashCode(), dataHash);
        }

        #endregion IEquatable<T>

        public static bool operator ==(InferredType left, InferredType right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((object)left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(InferredType left, InferredType right)
        {
            return !(left == right);
        }

        #region Private helpers

        private void InferTypeFromSchema(JsonSchema schema)
        {
            if (schema.Type == JsonType.String && schema.Format == FormatAttributes.DateTime)
            {
                SetClassName("System.DateTime");
            }
            else if (schema.Type != JsonType.None)
            {
                SetJsonType(schema.Type);
            }
            else if (schema.Reference != null)
            {
                InferTypeFromReference(schema.Reference);
            }
            else if (InferJsonTypeFromEnumValues(schema.Enum))
            {
                if (_jsonType != JsonType.None)
                {
                    // We were able to figure it out from the enum values.
                    return;
                }
            }
            else
            {
                SetJsonType(JsonType.None);
            }
        }

        private void InferTypeFromReference(UriOrFragment reference)
        {
            if (!reference.IsFragment)
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorOnlyDefinitionFragmentsSupported, reference));
            }

            string definitionName = GetDefinitionNameFromFragment(reference.Fragment);

            JsonSchema definitionSchema;
            if (!_rootSchema.Definitions.TryGetValue(definitionName, out definitionSchema)) // TODO: Check for null Definitions and add unit test
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorDefinitionDoesNotExist, definitionName));
            }

            if (definitionSchema.Type == JsonType.Boolean ||
                definitionSchema.Type == JsonType.Integer ||
                definitionSchema.Type == JsonType.Number ||
                definitionSchema.Type == JsonType.String)
            {
                SetJsonType(definitionSchema.Type);
            }
            else
            {
                SetClassName(definitionName.ToPascalCase());
            }
        }

        private bool InferJsonTypeFromEnumValues(object[] enumValues)
        {
            if (enumValues != null && enumValues.Any())
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
                    SetJsonType(jsonType);
                    return true;
                }
            }

            return false;
        }

        private void SetJsonType(JsonType jsonType)
        {
            _jsonType = jsonType;
            Kind = InferredTypeKind.JsonType;
        }

        private void SetClassName(string className)
        {
            _className = className;
            Kind = InferredTypeKind.ClassName;
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

        #endregion Private helpers
    }
}
