// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// JsonConverter class applied to those properties of the JsonSchema class which
    /// the JSON Schema specification requires to be strings.
    /// </summary>
    /// <remarks>
    /// This converter throws an exception if the property's value is not a string;
    /// otherwise it passes the value through unchanged.
    /// </remarks>
    internal class MustBeStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals(typeof(string));
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        private static readonly char[] s_pathSplitChars = new[] { '.' };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                JToken jToken = JToken.Load(reader);

                string propertyName = reader.Path?.Split(s_pathSplitChars).LastOrDefault()
                    ?? string.Empty;

                throw new SchemaValidationException(jToken, ErrorNumber.NotAString, propertyName, reader.TokenType);
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}