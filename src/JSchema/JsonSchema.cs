// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.JSchema
{
    public class JsonSchema : IEquatable<JsonSchema>
    {
        public static readonly Uri V4Draft = new Uri("http://json-schema.org/draft-04/schema#");

        public Uri Id { get; set; }

        [JsonProperty("$schema")]
        public Uri SchemaVersion { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public JsonType Type { get; set; }

        public Dictionary<string, JsonSchema> Properties { get; set; }

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
