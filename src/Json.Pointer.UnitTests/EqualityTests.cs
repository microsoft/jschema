// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class EqualityTests
    {
        private class EqualityTestCase
        {
            internal string Left { get; }
            internal string Right { get; }
            internal bool AreEqual { get; }

            internal EqualityTestCase(string left, string right, bool areEqual)
            {
                Left = left;
                Right = right;
                AreEqual = areEqual;
            }

            public override string ToString()
                => $"'{Left}' and '{Right}' {(AreEqual ? "are" : "are not")} equal";
        }

        private static readonly IReadOnlyCollection<EqualityTestCase> s_equalityTestCases = new ReadOnlyCollection<EqualityTestCase>(new List<EqualityTestCase>
        {
            new EqualityTestCase(string.Empty, string.Empty, areEqual: true),
            new EqualityTestCase("/a", string.Empty, areEqual: false),
            new EqualityTestCase(string.Empty, "/a", areEqual: false),
            new EqualityTestCase("/a", "/a", areEqual: true),
            new EqualityTestCase("/a", "/a/b", areEqual: false),
            new EqualityTestCase("/a", "/b/a", areEqual: false),
            new EqualityTestCase("/a/b", "/a/b", areEqual: true),
            new EqualityTestCase("/a//b", "/a//b", areEqual: true),
            new EqualityTestCase("/a/b", "/a//b", areEqual: false),
        });

        [Fact]
        public void EqualsMethod_GiveExpectedResults()
        {
            RunTestCases(
                actualResult: testCase => new JsonPointer(testCase.Left).Equals(new JsonPointer(testCase.Right)),
                expectedResult: testCase => testCase.AreEqual);
        }

        [Fact]
        public void EqualityOperator_GiveExpectedResults()
        {
            RunTestCases(
                actualResult: testCase => new JsonPointer(testCase.Left) == new JsonPointer(testCase.Right),
                expectedResult: testCase => testCase.AreEqual);
        }

        [Fact]
        public void InequalityOperator_GiveExpectedResults()
        {
            RunTestCases(
                actualResult: testCase => new JsonPointer(testCase.Left) != new JsonPointer(testCase.Right),
                expectedResult: testCase => !testCase.AreEqual);
        }

        private static void RunTestCases(
            Func<EqualityTestCase, bool> actualResult,
            Func<EqualityTestCase, bool> expectedResult)
        {
            var builder = new StringBuilder();

            foreach (EqualityTestCase testCase in s_equalityTestCases)
            {
                if (actualResult(testCase) != expectedResult(testCase))
                {
                    builder.AppendLine($"\u2022 {testCase}");
                }
            }

            builder.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{builder}");
        }
    }
}
