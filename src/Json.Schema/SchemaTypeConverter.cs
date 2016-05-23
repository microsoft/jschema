// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    internal class SchemaTypeConverter : JsonConverter
    {
        public static SchemaTypeConverter Instance = new SchemaTypeConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JTokenType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jToken = JToken.Load(reader);
            List<JTokenType> schemaTypes = new List<JTokenType>();

            if (jToken.Type == JTokenType.String)
            {
                string typeString = jToken.ToObject<string>();
                JTokenType schemaType = SchemaTypeFromString(typeString);
                if (schemaType != JTokenType.None)
                {
                    schemaTypes.Add(schemaType);
                }
                else
                {
                    serializer.CaptureError(jToken, ErrorNumber.InvalidTypeString, typeString);
                    return null;
                }
            }
            else if (jToken.Type == JTokenType.Array)
            {
                bool allValid = true;
                foreach (JToken elementToken in jToken as JArray)
                {
                    if (elementToken.Type == JTokenType.String)
                    {
                        string typeString = elementToken.ToObject<string>();
                        JTokenType schemaType = SchemaTypeFromString(typeString);
                        if (schemaType != JTokenType.None)
                        {
                            schemaTypes.Add(schemaType);
                        }
                        else
                        {
                            allValid = false;
                            serializer.CaptureError(elementToken, ErrorNumber.InvalidTypeString, typeString);
                        }
                    }
                    else
                    {
                        allValid = false;
                        serializer.CaptureError(elementToken, ErrorNumber.InvalidTypeType, elementToken.Type);
                    }
                }

                if (!allValid)
                {
                    return null;
                }
            }
            else
            {
                serializer.CaptureError(jToken, ErrorNumber.InvalidTypeType, jToken.Type);
                return null;
            }

            return schemaTypes;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string[] types = (value as JTokenType[]).Select(jtt => jtt.ToJsonSchemaName()).ToArray();

            if (types.Length == 1)
            {
                writer.WriteValue(types[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (string type in types)
                {
                    writer.WriteValue(type);
                }
            }
        }

        private static JTokenType SchemaTypeFromString(string s)
        {
            JTokenType schemaType = JTokenType.None;

            if (s == "number")
            {
                schemaType = JTokenType.Float;
            }
            else
            {
                s = s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);

                // Returns JTokenType.None if unrecognized.
                if (Enum.TryParse(s, out schemaType) && !IsValidSchemaType(schemaType))
                {
                    schemaType = JTokenType.None;
                }
            }

            return schemaType;
        }

        // This won't be necessary once we address #77 and make JsonSchemaType its own type.
        private static readonly ImmutableArray<JTokenType> s_validSchemaTypes = ImmutableArray.Create(
            JTokenType.Array,
            JTokenType.Boolean,
            JTokenType.Float,
            JTokenType.Integer,
            JTokenType.Object,
            JTokenType.String);

        private static bool IsValidSchemaType(JTokenType schemaType)
        {
            return s_validSchemaTypes.Contains(schemaType);
        }
    }
}
