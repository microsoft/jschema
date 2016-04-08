// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
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

        public static object[] SyntacticallyInvalidSchemaTestCases => new[]
        {
            new object[]
            {
@"{
  ""foo"": wrong
}"
            }
        };

        [Theory(DisplayName = "SchemaReader throws exception on syntactically invalid schemas")]
        [MemberData(nameof(SyntacticallyInvalidSchemaTestCases))]
        public void DetectsInvalidSchema(string jsonText)
        {
            Action action = () =>
            {
                using (var reader = new StringReader(jsonText))
                {
                    SchemaReader.ReadSchema(reader);
                }
            };

            action.ShouldThrow<JsonReaderException>()
                .Where(ex => ex.LineNumber == 2 && ex.LinePosition == 10);
        }

        public static object[] LogicallyInvalidSchemaTestCases => new[]
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

        [Theory(DisplayName = "SchemaReader throws exception on logically invalid schemas")]
        [MemberData(nameof(LogicallyInvalidSchemaTestCases))]
        public void DetectsLogicallyInvalidSchema(string testCaseName, string jsonText)
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
