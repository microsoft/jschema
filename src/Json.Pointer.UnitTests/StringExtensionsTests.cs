// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class StringExtensionsTests
    {
        [Fact(DisplayName = nameof(AtProperty_AppendsPropertyName))]
        public void AtProperty_AppendsPropertyName()
        {
            "/object".AtProperty("prop").Should().Be("/object/prop");
        }

        [Fact(DisplayName = nameof(AtProperty_EscapesPropertyName))]
        public void AtProperty_EscapesPropertyName()
        {
            "/object".AtProperty("p~/").Should().Be("/object/p~0~1");
        }

        [Fact(DisplayName = nameof(AtProperty_ThrowsOnNullPropertyName))]
        public void AtProperty_ThrowsOnNullPropertyName()
        {
            Action action = () => "/object".AtProperty(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact(DisplayName = nameof(AtIndex_AppendsZeroIndex))]
        public void AtIndex_AppendsZeroIndex()
        {
            "/array".AtIndex(0).Should().Be("/array/0");
        }

        [Fact(DisplayName = nameof(AtIndex_AppendPositiveIndex))]
        public void AtIndex_AppendPositiveIndex()
        {
            "/array".AtIndex(1).Should().Be("/array/1");
        }

        [Fact(DisplayName = nameof(AtIndex_ThrowsOnNegativeIndex))]
        public void AtIndex_ThrowsOnNegativeIndex()
        {
            Action action = () => "/array".AtIndex(-1);

            action.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
