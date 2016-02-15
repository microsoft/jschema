// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace MountBaker.JSchema.ObjectModel.Tests
{
    public class SchemaReaderTests
    {
        public static IEnumerable<object[]> TestCases = new[]
        {
            new object[]
            {
                "Empty",
                new JsonSchema()
            },

            new object[]
            {
                "BasicProperties",
                new JsonSchema
                {
                    SchemaVersion = JsonSchema.V4Draft
                }
            }
        };

        [Theory]
        [MemberData(nameof(TestCases))]
        public void CanReadSchema(string fileNameStem, JsonSchema expected)
        {
            string jsonText = File.ReadAllText($"TestData\\{fileNameStem}.schema.json");
            JsonSchema actual = SchemaReader.ReadSchema(jsonText);

            actual.Should().Be(expected);
        }
    }
}
