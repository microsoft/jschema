// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.Json.Schema.TestUtilities
{
    public static class TestUtil
    {
        public const string TestFilePath = @"C:\test.json";

        public static string ReadTestDataFile(string fileNameStem)
        {
            using (var reader = new StreamReader(GetTestDataStream(fileNameStem)))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetTestDataFilePath(string fileNameStem)
        {
            return $"TestData\\{fileNameStem}.schema.json";
        }

        public static Stream GetTestDataStream(string fileNameStem)
        {
            return new FileStream(GetTestDataFilePath(fileNameStem), FileMode.Open, FileAccess.Read);
        }

        public static JsonSchema CreateSchemaFromTestDataFile(string fileNameStem)
        {
            string jsonText = ReadTestDataFile(fileNameStem);
            return SchemaReader.ReadSchema(jsonText, GetTestDataFilePath(fileNameStem));
        }

#if SHOULD_WRITE_TEST_RESULT_FILES
        private static readonly string TestResultFilesDirectory = "TestResultFiles";
#endif

        public static void WriteTestResultFiles(string expected, string actual, string testName)
        {
#if SHOULD_WRITE_TEST_RESULT_FILES
            Directory.CreateDirectory(TestResultFilesDirectory);

            File.WriteAllText(Path.Combine(TestResultFilesDirectory, testName + ".expected.cs"), expected);
            File.WriteAllText(Path.Combine(TestResultFilesDirectory, testName + ".actual.cs"), actual);
#endif
        }
    }
}
