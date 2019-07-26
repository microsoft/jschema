// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Json.Schema
{
    public static class SchemaWriter
    {
        public static void WriteSchema(
            JsonWriter writer,
            JsonSchema schema,
            Formatting formatting = Formatting.Indented)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = formatting
            };

            var serializer = JsonSerializer.Create(settings);
            serializer.ContractResolver = new JsonSchemaContractResolver();
            serializer.Converters.Add(
                new StringEnumConverter
                {
                    CamelCaseText = true
                });

            serializer.Serialize(writer, schema);
        }

        public static void WriteSchema(
            TextWriter writer,
            JsonSchema schema,
            Formatting formatting = Formatting.Indented)
        {
            var stringWriter = new StringWriter();
            var jsonWriter = new JsonTextWriter(stringWriter);

            WriteSchema(jsonWriter, schema, formatting);

            // Change "$$ref" to "$ref" before we ask write it to the output.
            string output = RefProperty.ConvertToOutput(stringWriter.ToString());
            writer.Write(output);
        }
    }
}
