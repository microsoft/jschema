// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.JSchema.Tests
{
    public class SchemaWriterTests
    {
        public static IEnumerable<object[]> TestCases => ReaderWriter.TestCases;

        [Theory]
        [MemberData(nameof(TestCases))]
        public void CanWriteSchema(string fileNameStem, JsonSchema schema)
        {
            string expected = TestUtil.ReadTestDataFile(fileNameStem);

            string actual = null;
            using (var writer = new StringWriter())
            {
                SchemaWriter.WriteSchema(writer, schema, Formatting.Indented);
                actual = writer.ToString();
            }

            actual.Should().Be(expected);
        }
    }
}
