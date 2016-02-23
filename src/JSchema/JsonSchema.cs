// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.JSchema
{
    public class JsonSchema : IEquatable<JsonSchema>
    {
        public static readonly Uri V4Draft = new Uri("http://json-schema.org/draft-04/schema#");

        /// <summary>
        /// Gets or sets a URI that alters the resolution scope for the current schema and
        /// all of its descendants (until another Id is encountered).
        /// </summary>
        /// <remarks>
        /// See http://json-schema.org/latest/json-schema-core.html#anchor25 ("URI resolution
        /// scopes and dereferencing").
        /// </remarks>
        public UriOrFragment Id { get; set; }

        [JsonProperty("$schema")]
        public Uri SchemaVersion { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public JsonType Type { get; set; }

        /// <summary>
        /// Gets or sets an array containing the values that are valid for an object
        /// that conforms to the current schema.
        /// </summary>
        public object[] Enum { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema that applies to the array items, if the current
        /// schema is of array type.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="JsonType.Array"/>.
        /// </remarks>
        public JsonSchema Items { get; set; }

        /// <summary>
        /// Dictionary mapping valid property names to the sub-schemas to which they must
        /// conform.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="JsonType.Object"/>.
        /// </remarks>
        public Dictionary<string, JsonSchema> Properties { get; set; }

        /// <summary>
        /// Gets or sets an array containing the names of the property names that are
        /// required to be present.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="JsonType.Object"/>.
        /// </remarks>
        public string[] Required { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping schema names to sub-schemas which can be
        /// referenced by properties defined elsewhere in the current schema.
        /// </summary>
        public Dictionary<string, JsonSchema> Definitions { get; set; }

        /// <summary>
        /// Gets or sets the minimum valid number of elements in an array.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="JsonType.Array"/>.
        /// If this property is not specified, it is considered present with a value of 0.
        /// The type of this property is <code>int?</code>, rather than <code>int</code>
        /// with a default value of 0, is so that a schema that does not specify this
        /// property can be successfully round-tripped to and from the object model.
        /// </remarks>
        public int? MinItems { get; set; }

        /// <summary>
        /// Gets or sets the maximum valid number of elements in an array.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="JsonType.Array"/>.
        /// If this property is not specified, any number of items is valid.
        /// The type of this property is <code>int?</code>, rather than <code>int</code>
        /// with a default value of <code>Int32.MaxValue</code>, is so that a schema that
        /// does not specify this property can be successfully round-tripped to and from
        /// the object model.
        public int? MaxItems { get; set; }

        /// <summary>
        /// Gets or sets the URI of a schema that is incorporated by reference into
        /// the current schema.
        /// </summary>
        // See the RefProperty class for an explanation of our special treatment of
        // this property.
        [JsonProperty("$$ref")]
        public UriOrFragment Reference { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the required format of a string-valued property.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="JsonType.String"/>.
        /// </remarks>
        public string Format { get; set; }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonSchema);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(
                Id,
                SchemaVersion,
                Title,
                Description,
                Type,
                Enum,
                Items,
                Properties,
                Required,
                Definitions,
                Reference,
                MinItems,
                MaxItems,
                Format
                );
        }

        #endregion Object overrides

        #region IEquatable<T>

        public bool Equals(JsonSchema other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return Id == other.Id
                && (SchemaVersion == null
                        ? other.SchemaVersion == null
                        : SchemaVersion.EqualsWithFragments(other.SchemaVersion))
                && string.Equals(Title, other.Title, StringComparison.Ordinal)
                && string.Equals(Description, other.Description, StringComparison.Ordinal)
                && Type == other.Type
                && (Enum == null
                        ? other.Enum == null
                        : Enum.HasSameElementsAs(other.Enum))
                && (Items == null
                        ? other.Items == null
                        : Items.Equals(other.Items))
                && (Properties == null
                        ? other.Properties == null
                        : Properties.HasSameElementsAs(other.Properties))
                && (Required == null
                        ? other.Required == null
                        : Required.HasSameElementsAs(other.Required))
                && (Definitions == null
                        ? other.Definitions == null
                        : Definitions.HasSameElementsAs(other.Definitions))
                && (Reference == null
                        ? other.Reference == null
                        : Reference.Equals(other.Reference))
                && (MinItems == null
                        ? other.MinItems == null
                        : MinItems.Equals(other.MinItems))
                && (MaxItems == null
                        ? other.MaxItems == null
                        : MaxItems.Equals(other.MaxItems))
                && (Format == null
                        ? other.Format == null
                        : Format.Equals(other.Format));
        }

        #endregion

        public static bool operator ==(JsonSchema left, JsonSchema right)
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

        public static bool operator !=(JsonSchema left, JsonSchema right)
        {
            return !(left == right);
        }
    }
}
