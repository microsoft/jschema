// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;

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
        public static readonly InferredType None = new InferredType();

        private readonly JsonType _jsonType;
        private readonly string _className;

        /// <summary>
        /// Creates a new instance of the <see cref="InferredType"/> class from one of
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
        /// Creates a new instance of the <see cref="InferredType"/> class from a class name.
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
        public InferredTypeKind Kind { get; }

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
    }
}
