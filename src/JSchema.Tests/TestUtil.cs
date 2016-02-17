// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;

namespace MountBaker.JSchema.Tests
{
    internal static class TestUtil
    {
        internal static string ReadTestDataFile(string fileNameStem)
        {
            return File.ReadAllText($"TestData\\{fileNameStem}.schema.json");
        }

        internal static JsonSchema CreateSchemaFromTestDataFile(string fileNameStem)
        {
            string jsonText = ReadTestDataFile(fileNameStem);
            return SchemaReader.ReadSchema(jsonText);
        }
    }
}
