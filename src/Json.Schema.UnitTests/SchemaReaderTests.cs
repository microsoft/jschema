// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Json.Schema.TestUtilities;
using Xunit;
using Xunit.Abstractions;

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
                actual = SchemaReader.ReadSchema(reader, TestUtil.GetTestDataFilePath(fileNameStem));
            }

            actual.Should().Be(expected);
        }

        public static List<object[]> SyntacticallyInvalidSchemaTestCases => new List<object[]>
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
                    SchemaReader.ReadSchema(reader, TestUtil.TestFilePath);
                }
            };

            action.Should().Throw<JsonSyntaxException>()
                .Where(ex => ex.JsonReaderException.LineNumber == 2
                    && ex.JsonReaderException.LinePosition == 9);
        }

        public class LogicallyInvalidSchemaTestCase : IXunitSerializable
        {
            public LogicallyInvalidSchemaTestCase(
                string name,
                string schemaText,
                int numErrors = 1)
            {
                Name = name;
                SchemaText = schemaText;
                NumErrors = numErrors;
            }

            public LogicallyInvalidSchemaTestCase()
            {
                // Needed for serialization.
            }

            public string Name;
            public string SchemaText;
            public int NumErrors;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                NumErrors = info.GetValue<int>(nameof(NumErrors));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(NumErrors), NumErrors);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static TheoryData<LogicallyInvalidSchemaTestCase> LogicallyInvalidSchemaTestCases => new TheoryData<LogicallyInvalidSchemaTestCase>
        {
            new LogicallyInvalidSchemaTestCase(
                "additionalProperties is a non-Boolean primitive type",
@"{
  ""additionalProperties"": 2
}"
                ),

            new LogicallyInvalidSchemaTestCase(
                "additionalProperties is an array",
@"{
  ""additionalProperties"": []
}"
                ),

            new LogicallyInvalidSchemaTestCase(
                "title is not a string",
@"{
  ""title"": 2
}"
                ),

            new LogicallyInvalidSchemaTestCase(
                "title in nested schema is not a string",
@"{
  ""title"": ""Outer title"",
  ""properties"": {
    ""a"": {
      ""title"": []
    }
  }
}"
                ),

            new LogicallyInvalidSchemaTestCase(
                "description is not a string",
@"{
  ""description"": false
}"
                ),

            new LogicallyInvalidSchemaTestCase(
                "description in nested schema is not a string",
@"{
  ""properties"": {
    ""a"": {
      $ref: ""#/definitions/d""
    }
  },
  ""definitions"": {
    ""d"": {
      ""description"": {}
    }
  }
}"
                ),

                new LogicallyInvalidSchemaTestCase(
                    "multiple errors",
@"{
  ""title"": 1,
  ""additionalProperties"": 2
}",
                    2),

                new LogicallyInvalidSchemaTestCase(
                    "type is not a string or an array",
@"{
  ""type"": 2
}"
                    ),

                new LogicallyInvalidSchemaTestCase(
                    "type array has invalid element",
@"{
  ""type"": [ ""string"", false ]
}"
                    ),

                new LogicallyInvalidSchemaTestCase(
                    "dependency is neither an object nor an array",
@"{
  ""dependencies"": {
    ""a"": 2
  }
}")
        };

        [Theory(DisplayName = "SchemaReader throws on logically invalid schema")]
        [MemberData(nameof(LogicallyInvalidSchemaTestCases))]
        public void ThrowsOnLogicallyInvalidSchema(LogicallyInvalidSchemaTestCase test)
        {
            Action action = () =>
            {
                using (var reader = new StringReader(test.SchemaText))
                {
                    SchemaReader.ReadSchema(reader, TestUtil.TestFilePath);
                }
            };

            action.Should().Throw<SchemaValidationException>()
                .Where(ex => LogicallyInvalidSchemaExceptionPredicate(ex, test));
        }

        private bool LogicallyInvalidSchemaExceptionPredicate(SchemaValidationException ex, LogicallyInvalidSchemaTestCase test)
        {
            return ex.WrappedExceptions != null
                ? ex.WrappedExceptions.All(we => we.Args != null && we.JToken != null && we.ErrorNumber > 0)
                : ex.Args != null && ex.JToken != null && ex.ErrorNumber > 0;
        }
    }
}
