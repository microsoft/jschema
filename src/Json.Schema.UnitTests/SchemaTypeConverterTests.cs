// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Json.Schema.UnitTests
{
    public class SchemaTypeConverterTests
    {
        [Fact]
        public void SchemaTypeConverter_ShouldNotThrowIfObjectIsNull()
        {
            var exception = Record.Exception(() =>
            {
                using (var s = new StringWriter())
                {
                    using (var w = new JsonTextWriter(s))
                    {
                        SchemaTypeConverter.Instance.WriteJson(w, null, null);
                    }
                }
            });

            exception.Should().BeNull();
        }

        [Fact]
        public void SchemaTypeConverter_ShouldConvertArrayCorrectly()
        {
            var testCases = new[]
            {
                new
                {
                    List = new List<SchemaType> { SchemaType.Array },
                    Expected = @"""array"""
                },
                new
                {
                    List = new List<SchemaType> { SchemaType.Array, SchemaType.Boolean },
                    Expected = @"[""array"",""boolean""]"
                }
            };

            var sb = new StringBuilder();
            foreach (var testCase in testCases)
            {
                var stringBuilder = new StringBuilder();
                using (var s = new StringWriter(stringBuilder))
                {
                    using (var w = new JsonTextWriter(s))
                    {
                        SchemaTypeConverter.Instance.WriteJson(w, testCase.List, null);
                    }
                }

                var current = stringBuilder.ToString();
                if (current != testCase.Expected)
                {
                    sb.AppendLine($@"Test was expecting '{testCase.Expected}' but found '{current}'.");
                }
            }

            sb.Length.Should().Be(0, sb.ToString());
        }
    }
}
