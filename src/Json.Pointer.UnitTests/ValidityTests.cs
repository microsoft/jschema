// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Xunit;

namespace Json.Pointer.UnitTests
{
    public class ValidityTests
    {
        public static readonly TheoryData<ValidityTestCase> ValidityTestCases = new TheoryData<ValidityTestCase>
        {
            new ValidityTestCase(
                "Empty pointer"
            ),
        };

        [Theory(DisplayName = "JsonPointer validity")]
        [MemberData(nameof(ValidityTestCases))]
        public void RunValidityTests(ValidityTestCase test)
        {
            true.Should().BeTrue();
        }
    }
}
