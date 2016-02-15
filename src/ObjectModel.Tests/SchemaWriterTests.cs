// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace MountBaker.JSchema.ObjectModel.Tests
{
    public class SchemaWriterTests
    {
        public static IEnumerable<object[]> TestCases => ReaderWriter.TestCases;

        [Theory]
        [MemberData(nameof(TestCases))]
        public void CanWriteSchema(string fileNameStem, JsonSchema schema)
        {
            string expected = ReaderWriter.ReadTestDataFile(fileNameStem);

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
