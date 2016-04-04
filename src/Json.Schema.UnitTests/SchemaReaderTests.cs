// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Schema.UnitTests
{
    public class SchemaReaderTests
    {
        public static IEnumerable<object[]> ValidSchemaTestCases => ReaderWriter.TestCases;

        [Theory(DisplayName = "SchemaReader can read schemas")]
        [MemberData(nameof(ValidSchemaTestCases))]
        public void CanReadSchema(string fileNameStem, JsonSchema expected)
        {
            JsonSchema actual;
            using (var reader = new StreamReader(TestUtil.GetTestDataStream(fileNameStem)))
            {
                actual = SchemaReader.ReadSchema(reader);
            }

            actual.Should().Be(expected);
        }

        public static object[] InvalidSchemaTestCases => new[]
        {
            new object[]
            {
                "additionalProperties is a non-Boolean primitive type",
                @"
{
  ""additionalProperties"": 2
}"
            },

            new object[]
            {
                "additionalProperties is an array",
                @"
{
  ""additionalProperties"": []
}"
            }
        };

        [Theory(DisplayName = "SchemaReader throws exception on invalid schemas")]
        [MemberData(nameof(InvalidSchemaTestCases))]
        public void DetectsInvalidSchema(string testCaseName, string jsonText)
        {
            Action action = () =>
            {
                using (var reader = new StringReader(jsonText))
                {
                    SchemaReader.ReadSchema(reader);
                }
            };

            action.ShouldThrow<JSchemaException>();
        }
    }
}
