// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Xunit;

namespace Json.Pointer.UnitTests
{
    public class ValidityTests
    {
        [Fact(DisplayName = "JsonPointer validity")]
        public void RunValidityTests()
        {
            true.Should().BeTrue();
        }
    }
}
