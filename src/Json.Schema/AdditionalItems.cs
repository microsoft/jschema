// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Describes an additional array items beyond those specified by the schema's
    /// <code>items</code> property.
    /// </summary>
    /// <remarks>
    /// JSON schema v4, Sec. 5.3.1.1 says that a schema can have a property named
    /// <code>additionalItems</code> whose value can be either a boolean or a valid
    /// JSON schema.
    /// </remarks>
    public class AdditionalItems: IEquatable<AdditionalItems>
    {
        /// <summary>
        /// Initialized a new instance of the <see cref="AdditionalItems"/> class.
        /// </summary>
        public AdditionalItems()
        {
            // 8.2.2 If absent, "additionalItems" is considered present with an empty
            // schema as its value.
            Allowed = true;
            Schema = new JsonSchema();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalItems"/> class
        /// with the specified boolean value.
        /// </summary>
        /// <param name="allowed">
        /// A value indicating whether to allow array elements whose index is greater
        /// than the last index of the array specified by the <code>items</code> property.
        /// </param>
        public AdditionalItems(bool allowed)
        {
            Allowed = allowed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalItems"/> class
        /// with the specified schema.
        /// </summary>
        /// <param name="schema">
        /// The schema which applies to array elements whose index is greater than the
        /// last index of the array specified by the <code>items</code> property.
        /// </param>
        public AdditionalItems(JsonSchema schema)
        {
            Allowed = true;
            Schema = schema;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="AdditionalItems"/> class
        /// from an existing instance.
        /// </summary>
        /// <param name="other">
        /// The existing instance from which this instance is to be initialized.
        /// </param>
        public AdditionalItems(AdditionalItems other)
        {
            Allowed = other.Allowed;
            Schema = other.Schema != null
                ? new JsonSchema(other.Schema)
                : null;
        }

        /// <summary>
        /// Gets a value indicating whether to allow array elements whose index is
        /// greater than the last index of the array specified by the <code>items</code>
        /// property.
        /// </summary>
        public bool Allowed { get; }

        /// <summary>
        /// Gets the schema which applies to array elements whose index is greater than
        /// the last index of the array specified by the <code>items</code> property.
        /// </summary>
        public JsonSchema Schema { get; }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as AdditionalItems);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(new object[] { Allowed, Schema });
        }

        #endregion

        #region IEquatable<T>

        public bool Equals(AdditionalItems other)
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

        public static bool operator ==(AdditionalItems left, AdditionalItems right)
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

        public static bool operator !=(AdditionalItems left, AdditionalItems right)
        {
            return !(left == right);
        }
    }
}
