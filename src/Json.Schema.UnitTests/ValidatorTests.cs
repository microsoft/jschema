// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Json.Schema.UnitTests
{
    public class ValidatorTests
    {
        public class TestCase : IXunitSerializable
        {
            public TestCase(
                string name,
                string schemaText,
                string instanceText,
                params string[] expectedMessages)
            {
                Name = name;
                SchemaText = schemaText;
                InstanceText = instanceText;
                ExpectedMessages = expectedMessages;
            }

            public TestCase()
            {
                // Needer for deserializer.
            }

            public string Name;
            public string SchemaText;
            public string InstanceText;
            public string[] ExpectedMessages;

            public override string ToString()
            {
                return Name;
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                InstanceText = info.GetValue<string>(nameof(InstanceText));
                ExpectedMessages = info.GetValue<string[]>(nameof(ExpectedMessages));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(InstanceText), InstanceText);
                info.AddValue(nameof(ExpectedMessages), ExpectedMessages);
            }
        }

        public static TheoryData<TestCase> TestCases = new TheoryData<TestCase>
        {
            new TestCase(
                "Empty schema matches integer",
                "{}",
                "2"
            ),

            new TestCase(
                "Empty schema matches object",
                "{}",
                "{}"
            ),

            new TestCase(
                "Integer instance matches integer schema",
                @"{ ""type"": ""integer"" }",
                "2"
            ),

            new TestCase(
                "Non-integer instance does not match integer schema",
                @"{ ""type"": ""integer"" }",
                "\"s\"",
                Validator.FormatMessage(1, 3, ValidationErrorNumber.WrongTokenType, JTokenType.Integer, JTokenType.String)
            ),

            new TestCase(
                "Array instance matches array schema",
                @"{ ""type"": ""array"" }",
                "[]"
            ),

            new TestCase(
                "Non-array instance matches array schema",
                 @"{ ""type"": ""array"" }",
                "true",
                Validator.FormatMessage(1, 4, ValidationErrorNumber.WrongTokenType, JTokenType.Array, JTokenType.Boolean)
            ),

            new TestCase(
                "Integer instance matches number schema",
                @"{ ""type"": ""number"" }",
                "2"
            ),

            new TestCase(
                "Required property missing",
                @"{
                  ""type"": ""object"",
                  ""required"": [ ""a"", ""b"", ""c"" ]
                }",
                @"{
                  ""b"": true,
                  ""d"": true
                }",
                Validator.FormatMessage(1, 1, ValidationErrorNumber.RequiredPropertyMissing, "a"),
                Validator.FormatMessage(1, 1, ValidationErrorNumber.RequiredPropertyMissing, "c")
            ),

            new TestCase(
                "Object property matches its schema",
@"{
  ""type"": ""object"",
  ""properties"": {
  ""a"": {
    ""type"": ""boolean""
  }
  }
}",
@"{
  ""a"": true
}"
            ),

            new TestCase(
                "Object property does not match its schema",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""boolean""
    }
  }
}",
@"{
  ""a"": ""true""
}",
                Validator.FormatMessage(2, 14, ValidationErrorNumber.WrongTokenType, JTokenType.Boolean, JTokenType.String)
            ),
        };

        [Theory(DisplayName = "Validation")]
        [MemberData(nameof(TestCases))]
        public void ReportsMissingRequiredProperty(TestCase testCase)
        {
            JsonSchema schema = SchemaReader.ReadSchema(testCase.SchemaText);
            var target = new Validator(schema);
            string[] actualMessages = target.Validate(testCase.InstanceText);

            actualMessages.Length.Should().Be(testCase.ExpectedMessages.Length);
            actualMessages.Should().ContainInOrder(testCase.ExpectedMessages);
        }
    }
}
