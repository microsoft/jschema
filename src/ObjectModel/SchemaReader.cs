// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using Newtonsoft.Json;

namespace MountBaker.JSchema.ObjectModel
{
    public static class SchemaReader
    {
        public static JsonSchema ReadSchema(string jsonText)
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = new JsonSchemaContractResolver()
            };

            using (var sr = new StringReader(jsonText))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize<JsonSchema>(reader);
                }
            }
        }
    }
}
