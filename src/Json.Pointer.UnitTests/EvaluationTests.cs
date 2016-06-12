// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class EvaluationTests
    {
        private const string TestDocument =
@"{
  """": 1,
  ""a"": true,
  ""b"": {
    ""c"": [ 3, 2 ]
  },
  ""~"": ""abc"",
  ""/"": ""de"",
  ""~1"": ""fg"",
  ""arr1"": [
    ""el0"",
    {
      ""el2"": {
        ""xy"": ""z"",
        ""arr2"": [ 42, 54, 96 ]
      }
    }
  ]
}";

        public static readonly TheoryData<EvaluationTestCase> EvaluationTestCases = new TheoryData<EvaluationTestCase>
        {
            new EvaluationTestCase(
                "Empty pointer",
                TestDocument,
                string.Empty,
                true,
                TestDocument
            ),

            new EvaluationTestCase(
                "Existing property",
                TestDocument,
                "/a",
                true,
                "true"),

            new EvaluationTestCase(
                "Empty property name",
                TestDocument,
                "/",
                true,
                "1"),

            new EvaluationTestCase(
                "Missing property",
                TestDocument,
                "/x",
                false),

            new EvaluationTestCase(
                "Nested property",
                TestDocument,
                "/b/c",
                true,
                "[ 3, 2]"),

            new EvaluationTestCase(
                "Escaped tilde",
                TestDocument,
                "/~0",
                true,
                "\"abc\""),

            new EvaluationTestCase(
                "Escaped solidus",
                TestDocument,
                "/~1",
                true,
                "\"de\""),

            new EvaluationTestCase(
                "Evaluate ~1 before ~0",
                TestDocument,
                "/~01",
                true,
                "\"fg\""),

            new EvaluationTestCase(
                "Simple array element",
                TestDocument,
                "/arr1/0",
                true,
                "\"el0\""),

            new EvaluationTestCase(
                "Complex nested path",
                TestDocument,
                "/arr1/1/el2/arr2/2",
                true,
                "96")

            // TODO: invalid index
            // TODO: out of range index
            // TOOD: neither array nor object
        };

        private static readonly JTokenEqualityComparer s_comparer = new JTokenEqualityComparer();

        [Theory(DisplayName = "JsonPointer evaluation")]
        [MemberData(nameof(EvaluationTestCases))]
        public void RunEvaluationTests(EvaluationTestCase test)
        {
            JsonPointer jPointer = new JsonPointer(test.Pointer);
            JToken documentToken = JToken.Parse(test.Document);
            JToken actualResult = null;

            Action action = () => actualResult = jPointer.Evaluate(documentToken);

            if (test.Valid)
            {
                action.ShouldNotThrow();
                JToken expectedResult = JToken.Parse(test.Result);
                s_comparer.Equals(expectedResult, actualResult).Should().BeTrue();
            }
            else
            {
                action.ShouldThrow<ArgumentException>();
            }
        }
    }
}
