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
        public Uri Id { get; set; }

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
        public JsonSchema Items { get; set; }

        /// <summary>
        /// Dictionary mapping valid property names to the sub-schemas to which they must
        /// conform.
        /// </summary>
        public Dictionary<string, JsonSchema> Properties { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping schema names to sub-schemas which can be
        /// referenced by properties defined elsewhere in the current schema.
        /// </summary>
        public Dictionary<string, JsonSchema> Definitions { get; set; }

        /// <summary>
        /// Gets or sets the URI of a schema that is incorporated by reference into
        /// the current schema.
        /// </summary>
        // See the RefProperty class for an explanation of our special treatment of
        // this property.
        [JsonProperty("$$ref")]
        public Uri Reference { get; set; }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonSchema);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(Id, SchemaVersion, Title, Description, Type, Properties);
        }

        #endregion Object overrides

        #region IEquatable<T>

        public bool Equals(JsonSchema other)
        {
            if (other == null)
            {
                return false;
            }

            return Id == other.Id
                && SchemaVersion == other.SchemaVersion
                && string.Equals(Title, other.Title, StringComparison.Ordinal)
                && string.Equals(Description, other.Description, StringComparison.Ordinal)
                && Type == other.Type
                && Properties.HasSameElementsAs(other.Properties);
        }

        #endregion

        public static bool operator ==(JsonSchema left, JsonSchema right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(JsonSchema left, JsonSchema right)
        {
            return !(left == right);
        }
    }
}
