// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    internal class JTokenTypeConverter : JsonConverter
    {
        public static JTokenTypeConverter Instance = new JTokenTypeConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JTokenType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JTokenType jTokenType;

            string s = (string)reader.Value;
            if (s == "number")
            {
                jTokenType = JTokenType.Float;
            }
            else
            {
                s = s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
                jTokenType = (JTokenType)Enum.Parse(typeof(JTokenType), s);
            }

            return jTokenType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string s;

            var jTokenType = (JTokenType)value;
            if (jTokenType == JTokenType.Float)
            {
                s = "number";
            }
            else
            {
                s = jTokenType.ToString().ToLowerInvariant();
            }

            writer.WriteRawValue("\"" + s + "\"");
        }
    }
}
