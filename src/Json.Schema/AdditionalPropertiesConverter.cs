// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Converts a property of type <see cref="AdditionalProperties"/> to or from a string
    /// during serialization or deserialization.
    /// </summary>
    internal class AdditionalPropertiesConverter : JsonConverter
    {
        public static readonly AdditionalPropertiesConverter Instance = new AdditionalPropertiesConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AdditionalProperties);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jToken = JToken.Load(reader);

            if (jToken.Type == JTokenType.Boolean)
            {
                bool val = jToken.ToObject<bool>();
                return new AdditionalProperties(val);
            }
            else if (jToken.Type == JTokenType.Object)
            {
                JsonSchema schema = jToken.ToObject<JsonSchema>(serializer);
                return new AdditionalProperties(schema);
            }
            else
            {
                serializer.CaptureError(jToken, ErrorNumber.InvalidAdditionalPropertiesType, jToken.Type);
                return null;
            }
        }

        /// <summary>
        /// Writes the JSON representation of an object.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="JsonWriter"/> to write to.
        /// </param>
        /// <param name="value">
        /// The object to write.
        /// </param>
        /// <param name="serializer">
        /// The calling serializer.
        /// </param>
        /// <remarks>
        /// An <see cref="AdditionalProperties"/> object can hold either a JSON schema or
        /// a Boolean value. If <see cref="AdditionalProperties.Schema"/> is non-null,
        /// write it out; otherwise write out the Boolean value.
        /// </remarks>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var additionalProperties = (AdditionalProperties)value;

            if (additionalProperties.Schema != null)
            {
                SchemaWriter.WriteSchema(writer, additionalProperties.Schema);
            }
            else
            {
                JValue v = (JValue)JToken.FromObject(additionalProperties.Allowed);
                v.WriteTo(writer);
            }
        }
    }
}
