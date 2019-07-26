﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    internal class SchemaTypeConverter : ErrorAccumulatingConverter
    {
        public SchemaTypeConverter(SchemaValidationErrorAccumulator errorAccumulator)
            : base(errorAccumulator) { }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SchemaType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jToken = JToken.Load(reader);
            var schemaTypes = new List<SchemaType>();

            if (jToken.Type == JTokenType.String)
            {
                string typeString = jToken.ToObject<string>();
                SchemaType schemaType = SchemaTypeFromString(typeString);
                if (schemaType != SchemaType.None)
                {
                    schemaTypes.Add(schemaType);
                }
                else
                {
                    ErrorAccumulator.AddError(jToken, ErrorNumber.InvalidTypeString, typeString);
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
                        SchemaType schemaType = SchemaTypeFromString(typeString);
                        if (schemaType != SchemaType.None)
                        {
                            schemaTypes.Add(schemaType);
                        }
                        else
                        {
                            allValid = false;
                            ErrorAccumulator.AddError(elementToken, ErrorNumber.InvalidTypeString, typeString);
                        }
                    }
                    else
                    {
                        allValid = false;
                        ErrorAccumulator.AddError(elementToken, ErrorNumber.InvalidTypeType, elementToken.Type);
                    }
                }

                if (!allValid)
                {
                    return null;
                }
            }
            else
            {
                ErrorAccumulator.AddError(jToken, ErrorNumber.InvalidTypeType, jToken.Type);
            }

            return schemaTypes;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string[] types = (value as SchemaType[]).Select(st => st.ToString().ToLowerInvariant()).ToArray();

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

        private static SchemaType SchemaTypeFromString(string s)
        {
            s = s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);

            // Returns SchemaType.None if unrecognized.
            SchemaType schemaType;
            Enum.TryParse(s, out schemaType);

            return schemaType;
        }
    }
}
