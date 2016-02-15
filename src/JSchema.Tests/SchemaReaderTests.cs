// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace MountBaker.JSchema.Tests
{
    public class SchemaReaderTests
    {
        public static IEnumerable<object[]> TestCases => ReaderWriter.TestCases;

        [Theory]
        [MemberData(nameof(TestCases))]
        public void CanReadSchema(string fileNameStem, JsonSchema expected)
        {
            string jsonText = ReaderWriter.ReadTestDataFile(fileNameStem);
            JsonSchema actual = SchemaReader.ReadSchema(jsonText);

            actual.Should().Be(expected);
        }
    }
}
