// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Converts a property of type <see cref="Items"/> to or from a string
    /// during serialization or deserialization.
    /// </summary>
    public class ItemsConverter : JsonConverter
    {
        public static readonly ItemsConverter Instance = new ItemsConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Items);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jToken = JToken.Load(reader);

            if (jToken.Type == JTokenType.Object)
            {
                JsonSchema schema = jToken.ToObject<JsonSchema>(serializer);
                return new Items(schema);
            }
            else if (jToken.Type == JTokenType.Array)
            {
                IList<JsonSchema> schemas = jToken.ToObject<IList<JsonSchema>>(serializer);
                return new Items(schemas);
            }
            else
            {
                SchemaValidationErrorAccumulator.Instance.AddError(jToken, ErrorNumber.InvalidItemsType, jToken.Type);
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
            var items = (Items)value;

            if (items.SingleSchema)
            {
                SchemaWriter.WriteSchema(writer, items.Schema);
            }
            else
            {
                writer.WriteStartArray();
                foreach (JsonSchema schema in items.Schemas)
                {
                    SchemaWriter.WriteSchema(writer, items.Schema);
                }

                writer.WriteEndArray();
            }
        }
    }
}
