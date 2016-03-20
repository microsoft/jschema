// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Schema.Tests
{
    public class UriOrFragmentTests
    {
        [Fact(DisplayName = "UriOrFragment accepts fragment")]
        public void AcceptsFragment()
        {
            var target = new UriOrFragment("#fragment");

            target.IsFragment.Should().BeTrue();
            target.IsUri.Should().BeFalse();
            target.Fragment.Should().Be("#fragment");
        }

        [Fact(DisplayName = "UriOrFragment accepts empty fragment")]
        public void AcceptsEmptyFragment()
        {
            var target = new UriOrFragment("#");

            target.IsFragment.Should().BeTrue();
            target.IsUri.Should().BeFalse();
            target.Fragment.Should().Be("#");
        }

        [Fact(DisplayName = "UriOrFragment accepts absolute URI")]
        public void AcceptsAbsoluteUri()
        {
            var target = new UriOrFragment("http://www.example.com/products.html");

            target.IsUri.Should().BeTrue();
            target.IsFragment.Should().BeFalse();
            target.Uri.Should().Be(new Uri("http://www.example.com/products.html"));
        }

        [Fact(DisplayName = "UriOrFragment accepts relative URI")]
        public void AcceptsRelativeUri()
        {
            var target = new UriOrFragment("products.html");

            target.IsUri.Should().BeTrue();
            target.IsFragment.Should().BeFalse();
            target.Uri.Should().Be(new Uri("products.html", UriKind.Relative));
        }

        [Fact(DisplayName = "UriOrFragment accepts relative URI with fragment")]
        public void AcceptsRelativeUriWithFragment()
        {
            var target = new UriOrFragment("products.html#fragment");

            target.IsUri.Should().BeTrue();
            target.IsFragment.Should().BeFalse();
            
            // Uri.Equals compare fragments on relative URIs.
            target.Uri.Should().Be(new Uri("products.html#fragment", UriKind.Relative));
        }

        [Fact(DisplayName = "UriOrFragment accepts absolute URI with fragment")]
        public void AcceptsAbsoluteUriWithFragment()
        {
            var target = new UriOrFragment("http://host/products.html#fragment");

            target.IsUri.Should().BeTrue();
            target.IsFragment.Should().BeFalse();

            // Uri.Equals does not compare fragments on absolute URIs...
            target.Uri.Should().Be(new Uri("http://host/products.html", UriKind.Absolute));

            // ... so we'll do it by hand.
            target.Uri.Fragment.Should().Be("#fragment");
        }

        [Fact(DisplayName = "UriOrFragment throws on invalid URI")]
        public void ThrowsOnInvalidUri()
        {
            UriOrFragment target;
            Action action = () =>
            {
                // Bad character in port number. It's hard to find a good example of an invalid
                // URI because System.Uri implicitly URL-escapes invalid characters in most
                // locations. But it can't rescue a bad port number.
                target = new UriOrFragment(@"http://www.example.com:80y/products.html");
            };

            action.ShouldThrow<UriFormatException>();
        }

        public static readonly object[] EqualityTestCases = new object[]
        {
            new object[]
            {
                "Compare fragment to same fragment",
                new UriOrFragment("#f"),
                new UriOrFragment("#f"),
                true
            },

            new object[]
            {
                "Compare fragment to different fragment",
                new UriOrFragment("#f"),
                new UriOrFragment("#g"),
                false
            },

            new object[]
            {
                "Compare fragment to null",
                new UriOrFragment("#fragment"),
                null,
                false
            },

            new object[]
            {
                "Compare fragment to URI",
                new UriOrFragment("#f"),
                new UriOrFragment("f"),
                false
            },

            new object[]
            {
                "Compare relative URI to same relative URI",
                new UriOrFragment("u"),
                new UriOrFragment("u"),
                true
            },

            new object[]
            {
                "Compare relative URI to different relative URI",
                new UriOrFragment("u"),
                new UriOrFragment("v"),
                false
            },

            new object[]
            {
                "Compare relative URIs with same fragments",
                new UriOrFragment("u#f"),
                new UriOrFragment("u#f"),
                true
            },

            new object[]
            {
                "Compare relative URIs with different fragments",
                new UriOrFragment("u#f"),
                new UriOrFragment("u#g"),
                false
            },

            new object[]
            {
                "Compare absolute URI to same absolute URI",
                new UriOrFragment("http://u"),
                new UriOrFragment("http://u"),
                true
            },

            new object[]
            {
                "Compare absolute URI to different absolute URI",
                new UriOrFragment("http://u"),
                new UriOrFragment("http://v"),
                false
            },

            new object[]
            {
                "Compare absolute URIs with same fragments",
                new UriOrFragment("http://u#f"),
                new UriOrFragment("http://u#f"),
                true
            },

            new object[]
            {
                "Compare absolute URIs with different fragments",
                new UriOrFragment("http://u#f"),
                new UriOrFragment("http://u#g"),
                false
            },

            new object[]
            {
                "Compare URI to null",
                new UriOrFragment("uri"),
                null,
                false
            }
        };

        [Theory(DisplayName = "UriOrFragment equality tests")]
        [MemberData(nameof(EqualityTestCases))]
        public void EqualityTests(string testName, UriOrFragment left, UriOrFragment right, bool shouldBeEqual)
        {
            left.Equals(right).Should().Be(shouldBeEqual);
            (left == right).Should().Be(shouldBeEqual);
            (left != right).Should().Be(!shouldBeEqual);
        }
    }
}
