// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class ValidityTests
    {
        public static readonly TheoryData<ValidityTestCase> ValidityTestCases = new TheoryData<ValidityTestCase>
        {
            new ValidityTestCase(
                "Empty pointer",
                string.Empty,
                true
            ),
        };

        [Theory(DisplayName = "JsonPointer validity")]
        [MemberData(nameof(ValidityTestCases))]
        public void RunValidityTests(ValidityTestCase test)
        {
            JsonPointer jPointer = null;
            
            Action action = () => jPointer = new JsonPointer(test.Value);

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
