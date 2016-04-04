// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Describes any additional properties allowed by the JSON schema.
    /// </summary>
    /// <remarks>
    /// JSON schema v4, Sec. 5.4.4.1, says that a schema can have a property named
    /// <code>additionalProperties</code>, whose value is either a Boolean or a JSON
    /// schema.
    /// </remarks>
    public class AdditionalProperties: IEquatable<AdditionalProperties>
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="AdditionalProperties"/> class
        /// with the specified Boolean value.
        /// </summary>
        /// <param name="allowed">
        /// A value indicating whether the schema allows additional properties.
        /// </param>
        public AdditionalProperties(bool allowed)
        {
            Allowed = allowed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalProperties"/> class
        /// with the specified schema.
        /// </summary>
        /// <param name="schema">
        /// A schema to which any additional properties defined by the schema must
        /// conform;
        /// </param>
        public AdditionalProperties(JsonSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            Schema = schema;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="AdditionalProperties"/> class
        /// from an existing instance.
        /// </summary>
        /// <param name="other">
        /// The existing instance from which this instance is to be initialized.
        /// </param>
        public AdditionalProperties(AdditionalProperties other)
        {
            Allowed = other.Allowed;
            Schema = other.Schema != null
                ? new JsonSchema(other.Schema)
                : null;
        }

        /// <summary>
        /// Gets a value indicating whether additional properties are allowed, because
        /// the schema specified <code>additionalProperties</code> either with a Boolean
        /// value of <code>true</code> or with a schema.
        /// </summary>
        public bool Allowed { get; }

        /// <summary>
        /// Gets the schema specified by the <code>additionalValues</code> property, or
        /// <code>null</code> if the schema did not specify <code>additionalValues</code>,
        /// or specified it with a Boolean value.
        /// </summary>
        public JsonSchema Schema { get; }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as AdditionalProperties);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(Allowed, Schema);
        }

        #endregion

        #region IEquatable<T>

        public bool Equals(AdditionalProperties other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return Allowed == other.Allowed
                && (Schema == null
                        ? other.Schema == null
                        : Schema.Equals(other.Schema));
        }

        #endregion

        public static bool operator ==(AdditionalProperties left, AdditionalProperties right)
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

        public static bool operator !=(AdditionalProperties left, AdditionalProperties right)
        {
            return !(left == right);
        }
    }
}
