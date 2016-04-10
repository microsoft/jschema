// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.Json.Schema;
using Newtonsoft.Json.Linq;
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
    ""type"": ""integer""
}",

                "2",

                new string[0]
            },

            new object[]
            {
@"{
    ""type"": ""integer""
}",

                "\"s\"",

                new string[]
                {
                    ValidatingJsonWalker.FormatMessage(
                        1, 3, ValidationErrorNumber.WrongTokenType, JTokenType.Integer, JTokenType.String)
                }
            },

            new object[]
            {
@"{
    ""type"": ""array""
}",

                "[]",

                new string[0]
            },

            new object[]
            {
@"{
    ""type"": ""array""
}",

                "true",

                new string[]
                {
                    ValidatingJsonWalker.FormatMessage(
                        1, 4, ValidationErrorNumber.WrongTokenType, JTokenType.Array, JTokenType.Boolean)
                }
            },

            new object[]
            {
@"{
    ""type"": ""number""
}",

                "2",

                new string[0]
            },
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
