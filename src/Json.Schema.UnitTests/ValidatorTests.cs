// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using System.Linq;
using Xunit;

namespace Microsoft.Json.Schema.UnitTests
{
    public class ValidatorTests
    {
        public static object[] TestCases = new object[]
        {
            new object[]
            {
@"{
}",

@"{
}",
                new string[0]
            },

            new object[]
            {
@"{
  ""properties"": {
    ""a"": {
        ""type"": ""integer""
    },
    ""b"": {
        ""type"": ""integer""
    },
    ""c"": {
        ""type"": ""integer""
    }
  },
  ""required"": [""a"", ""c""]
}",

@"{
  ""a"": 1,
  ""c"": 2
}",

                new string[0]
            },

            new object[]
            {
@"{
  ""properties"": {
    ""a"": {
        ""type"": ""integer""
    },
    ""b"": {
        ""type"": ""integer""
    },
    ""c"": {
        ""type"": ""integer""
    }
  },
  ""required"": [""a"", ""c""]
}",

@"{
  ""b"": 2
}",

                new string[]
                {
                    "The object at path \"\" does not contain the required property \"a\".",
                    "The object at path \"\" does not contain the required property \"c\"."
                }
            }
        };

        [Theory(DisplayName = "Validator tests")]
        [MemberData(nameof(TestCases))]
        public void ReportsMissingRequiredProperty(string schemaText, string instanceText, string[] expectedMessages)
        {
            JsonSchema schema = SchemaReader.ReadSchema(schemaText);
            var target = new Validator(schema);
            string[] actualMessages = target.Validate(instanceText);

            actualMessages.Length.Should().Be(expectedMessages.Length);
            actualMessages.Should().ContainInOrder(expectedMessages);
        }
    }
}
