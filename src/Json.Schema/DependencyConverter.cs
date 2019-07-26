// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Converts a property of type <see cref="Dependency"/> to or from a string
    /// during serialization or deserialization.
    /// </summary>
    internal class DependencyConverter : ErrorAccumulatingConverter
    {
        public DependencyConverter(SchemaValidationErrorAccumulator errorAccumulator)
            : base(errorAccumulator) { }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dependency);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jToken = JToken.Load(reader);

            if (jToken.Type == JTokenType.Object)
            {
                JsonSchema schema = jToken.ToObject<JsonSchema>(serializer);
                return new Dependency(schema);
            }
            else if (jToken.Type == JTokenType.Array)
            {
                IList<string> propertyDependencies = new List<string>();
                foreach (JToken elementToken in jToken as JArray)
                {
                    if (elementToken.Type == JTokenType.String)
                    {
                        propertyDependencies.Add(elementToken.ToObject<string>(serializer));
                    }
                    else
                    {
                        ErrorAccumulator.AddError(elementToken, ErrorNumber.InvalidPropertyDependencyType, elementToken.Type);
                    }
                }

                return new Dependency(propertyDependencies);
            }
            else
            {
                ErrorAccumulator.AddError(jToken, ErrorNumber.InvalidDependencyType, jToken.Type);
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
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            var dependencies = (Dictionary<string, Dependency>)value;
            foreach (string key in dependencies.Keys)
            {
                writer.WritePropertyName(key);

                Dependency dependency = dependencies[key];
                if (dependency.SchemaDependency != null)
                {
                    SchemaWriter.WriteSchema(writer, dependency.SchemaDependency);
                }
                else if (dependency.PropertyDependencies != null)
                {
                    writer.WriteStartArray();
                    foreach (string arrayElement in dependency.PropertyDependencies)
                    {
                        writer.WriteValue(arrayElement);
                    }

                    writer.WriteEndArray();
                }
            }

            writer.WriteEndObject();
        }
    }
}
