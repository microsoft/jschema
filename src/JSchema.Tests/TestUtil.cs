// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.JSchema.Tests
{
    internal static class TestUtil
    {
        internal static string ReadTestDataFile(string fileNameStem)
        {
            using (var reader = new StreamReader(GetTestDataStream(fileNameStem)))
            {
                return reader.ReadToEnd();
            }
        }

        internal static Stream GetTestDataStream(string fileNameStem)
        {
            return new FileStream($"TestData\\{fileNameStem}.schema.json", FileMode.Open, FileAccess.Read);
        }

        internal static JsonSchema CreateSchemaFromTestDataFile(string fileNameStem)
        {
            string jsonText = ReadTestDataFile(fileNameStem);
            return SchemaReader.ReadSchema(jsonText);
        }
    }
}
