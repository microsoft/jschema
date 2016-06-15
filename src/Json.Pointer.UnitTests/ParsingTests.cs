// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class ParsingTests
    {
        public static readonly TheoryData<ParsingTestCase> ParsingTestCases = new TheoryData<ParsingTestCase>
        {
            new ParsingTestCase(
                "Empty pointer",
                string.Empty,
                true
            ),

            new ParsingTestCase(
                "Empty token",
                "/",
                true,
                string.Empty),

            new ParsingTestCase(
                "Single token",
                "/a",
                true,
                "a"),

            new ParsingTestCase(
                "Multiple tokens",
                "/a/12/bc",
                true,
                "a", "12", "bc"),

            new ParsingTestCase(
                "Multiple tokens with empty token",
                "/a//bc",
                true,
                "a", string.Empty, "bc"),

            new ParsingTestCase(
                "Escaped characters",
                "/~0/~1",
                true,
                "~0", "~1"),

            new ParsingTestCase(
                "Does not start with '/'",
                "a",
                false),

            new ParsingTestCase(
                "Invalid escape sequence ~2",
                "/~2",
                false),

            new ParsingTestCase(
                "Invalid escape sequence ~~",
                "/~2",
                false),
        };

        [Theory(DisplayName = "JsonPointer parsing")]
        [MemberData(nameof(ParsingTestCases))]
        public void RunParsingTests(ParsingTestCase test)
        {
            RunTestCase(test, JsonPointerRepresentation.Normal);
        }

        public static readonly TheoryData<ParsingTestCase> UriFragmentParsingTestCases = new TheoryData<ParsingTestCase>
        {
            new ParsingTestCase(
                "Does not start with '#'",
                "/a",
                false),

            new ParsingTestCase(
                "Empty pointer",
                "#",
                true
            ),

            new ParsingTestCase(
                "Empty token",
                "#/",
                true,
                string.Empty),

            new ParsingTestCase(
                "Single token",
                "#/a",
                true,
                "a"),

            new ParsingTestCase(
                "Multiple tokens with empty token",
                "#/a//bc",
                true,
                "a", string.Empty, "bc"),

            new ParsingTestCase(
                "Escaped characters",
                "#/~0/~1",
                true,
                "~0", "~1"),

            new ParsingTestCase(
                "Pointer does not start with '/'",
                "#a",
                false),

            new ParsingTestCase(
                "Fragment: contains percent-encoded characters",
                "#/%20ab/c%25d/ef%5E/g%7Ch/i%5Cj/%22%22",
                true,
                " ab", "c%d", "ef^", "g|h", @"i\j", "\"\"")
        };

        [Theory(DisplayName = "JsonPointer fragment parsing")]
        [MemberData(nameof(UriFragmentParsingTestCases))]
        public void RunUriFragmentParsingTests(ParsingTestCase test)
        {
            RunTestCase(test, JsonPointerRepresentation.UriFragment);
        }

        public static readonly TheoryData<ParsingTestCase> JsonStringParsingTestCases = new TheoryData<ParsingTestCase>
        {
            new ParsingTestCase(
                "Escaped characters",
                @"/\\\""/a",
                true,
                @"\""", "a"),
        };

        [Theory(DisplayName = "JsonPointer JSON string parsing")]
        [MemberData(nameof(JsonStringParsingTestCases))]
        public void RunJsonStringParsingTests(ParsingTestCase test)
        {
            RunTestCase(test, JsonPointerRepresentation.JsonString);
        }

        private void RunTestCase(ParsingTestCase test, JsonPointerRepresentation format)
        {
            JsonPointer jPointer = null;

            Action action = () => jPointer = new JsonPointer(test.Value, format);

            if (test.Valid)
            {
                action.ShouldNotThrow();
                jPointer.ReferenceTokens.Should().ContainInOrder(test.ReferenceTokens);
                jPointer.ReferenceTokens.Length.Should().Be(test.ReferenceTokens.Length);
            }
            else
            {
                action.ShouldThrow<ArgumentException>();
            }
        }
    }
}
