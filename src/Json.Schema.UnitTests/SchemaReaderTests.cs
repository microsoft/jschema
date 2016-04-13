// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
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

        public class LogicallyInvalidSchemaTestCase : IXunitSerializable
        {
            public LogicallyInvalidSchemaTestCase(
                string name,
                string schemaText)
            {
                Name = name;
                SchemaText = schemaText;
            }

            public LogicallyInvalidSchemaTestCase()
            {
                // Needed for serialization.
            }

            public string Name;
            public string SchemaText;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
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
                )
        };

        [Theory(DisplayName = "SchemaReader throws on logically invalid schema")]
        [MemberData(nameof(LogicallyInvalidSchemaTestCases))]
        public void ThrowsOnLogicallyInvalidSchema(LogicallyInvalidSchemaTestCase test)
        {
            Action action = () =>
            {
                using (var reader = new StringReader(test.SchemaText))
                {
                    SchemaReader.ReadSchema(reader);
                }
            };

            action.ShouldThrow<ApplicationException>();
        }
    }
}
