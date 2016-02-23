// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.JSchema.Tests
{
    public class JsonSchemaTests
    {
        public static readonly object[] EqualityTestCases =
        {
            new object[]
            {
                "Empty schemas",
                new JsonSchema(),
                new JsonSchema(),
                true
            },

            new object[]
            {
                "All properties equal",
                new JsonSchema
                {
                    Id = new UriOrFragment("http://x/y#"),
                    SchemaVersion = new Uri("http://z"),
                    Title = "x",
                    Enum = new object[] { "a", "b" },
                    Items = new JsonSchema { Type = JsonType.Integer },
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["a"] = new JsonSchema { Type = JsonType.Object },
                        ["b"] = new JsonSchema { Type = JsonType.String }
                    },
                    Required = new string[]
                    {
                        "a"
                    },
                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["c"] = new JsonSchema { Type = JsonType.Integer },
                        ["d"] = new JsonSchema { Type = JsonType.Boolean }
                    },
                    Reference = new UriOrFragment("http://www.example.com/schema/#"),
                    MinItems = 1,
                    MaxItems = 3,
                    Format = FormatAttributes.DateTime
                },
                new JsonSchema
                {
                    Id = new UriOrFragment("http://x/y#"),
                    SchemaVersion = new Uri("http://z"),
                    Title = "x",
                    Enum = new object[] { "a", "b" },
                    Items = new JsonSchema { Type = JsonType.Integer },
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["a"] = new JsonSchema { Type = JsonType.Object },
                        ["b"] = new JsonSchema { Type = JsonType.String }
                    },
                    Required = new string[]
                    {
                        "a"
                    },
                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["c"] = new JsonSchema { Type = JsonType.Integer },
                        ["d"] = new JsonSchema { Type = JsonType.Boolean }
                    },
                    Reference = new UriOrFragment("http://www.example.com/schema/#"),
                    MinItems = 1,
                    MaxItems = 3,
                    Format = FormatAttributes.DateTime
                },
                true
            },

            new object[]
            {
                "Different Ids",
                new JsonSchema
                {
                    Id = new UriOrFragment("http://x/y#"),
                },
                new JsonSchema
                {
                    Id = new UriOrFragment("http://x/y#a"),
                },
                false
            },

            new object[]
            {
                "Null and non-null Ids",
                new JsonSchema
                {
                    Id = null,
                },
                new JsonSchema
                {
                    Id = new UriOrFragment("http://x/y#"),
                },
                false
            },

            new object[]
            {
                "Different schema versions",
                new JsonSchema
                {
                    SchemaVersion = new Uri("http://z")
                },
                new JsonSchema
                {
                    SchemaVersion = new Uri("http://q")
                },
                false
            },

            new object[]
            {
                "Null and non-null schema versions",
                new JsonSchema
                {
                    SchemaVersion = null
                },
                new JsonSchema
                {
                    SchemaVersion = new Uri("http://z")
                },
                false
            },

            new object[]
            {
                "Different titles",
                new JsonSchema
                {
                    Title = "x"
                },
                new JsonSchema
                {
                    Title = "y"
                },
                false
            },

            new object[]
            {
                "Null and non-null titles",
                new JsonSchema
                {
                    Title = null
                },
                new JsonSchema
                {
                    Title = "y"
                },
                false
            },

            new object[]
            {
                "Different enum lists",
                new JsonSchema
                {
                    Enum = new object[] { "a", "b" }
                },
                new JsonSchema
                {
                    Enum = new object[] { "a", "c" }
                },
                false
            },

            new object[]
            {
                "Same enum lists in different orders",
                new JsonSchema
                {
                    Enum = new object[] { "a", "b" }
                },
                new JsonSchema
                {
                    Enum = new object[] { "b", "a" }
                },
                true
            },

            new object[]
            {
                "Null and non-null enum lists",
                new JsonSchema
                {
                    Enum = null
                },
                new JsonSchema
                {
                    Enum = new object[] { "a", "b" }
                },
                false
            },

            new object[]
            {
                "Different item schemas",
                new JsonSchema
                {
                    Items = new JsonSchema { Type = JsonType.Integer }
                },
                new JsonSchema
                {
                    Items = new JsonSchema { Type = JsonType.Boolean }
                },
                false
            },

            new object[]
            {
                "Null and non-null item schemas",
                new JsonSchema
                {
                    Items = null
                },
                new JsonSchema
                {
                    Items = new JsonSchema { Type = JsonType.Boolean }
                },
                false
            },

            new object[]
            {
                "Different property sets",
                new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["a"] = new JsonSchema { Type = JsonType.Object },
                        ["b"] = new JsonSchema { Type = JsonType.String }
                    }
                },
                new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["a"] = new JsonSchema { Type = JsonType.Object },
                        ["b"] = new JsonSchema { Type = JsonType.Number }
                    }
                },
                false
            },

            new object[]
            {
                "Null and non-null property sets",
                new JsonSchema
                {
                    Properties = null
                },
                new JsonSchema
                {
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["a"] = new JsonSchema { Type = JsonType.Object },
                        ["b"] = new JsonSchema { Type = JsonType.Number }
                    }
                },
                false
            },

            new object[]
            {
                "Different required properties",
                new JsonSchema
                {
                    Required = new string[] { "a", "b" }
                },
                new JsonSchema
                {
                    Required = new string[] { "a" }
                },
                false
            },

            new object[]
            {
                "Null and non-null required properties",
                new JsonSchema
                {
                    Required = null
                },
                new JsonSchema
                {
                    Required = new string[] { "a" }
                },
                false
            },

            new object[]
            {
                "Different definitions dictionaries",
                new JsonSchema
                {
                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["c"] = new JsonSchema { Type = JsonType.Integer },
                        ["d"] = new JsonSchema { Type = JsonType.Boolean }
                    }
                },
                new JsonSchema
                {
                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["e"] = new JsonSchema { Type = JsonType.Integer },
                        ["f"] = new JsonSchema { Type = JsonType.Boolean }
                    }
                },
                false
            },

            new object[]
            {
                "Null and non-null definitions dictionaries",
                new JsonSchema
                {
                    Definitions = null
                },
                new JsonSchema
                {
                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["e"] = new JsonSchema { Type = JsonType.Integer },
                        ["f"] = new JsonSchema { Type = JsonType.Boolean }
                    }
                },
                false
            },

            new object[]
            {
                "Different references",
                new JsonSchema
                {
                    Reference = new UriOrFragment("schema/#")
                },
                new JsonSchema
                {
                    Reference = new UriOrFragment("schema/#x")
                },
                false
            },

            new object[]
            {
                "Null and non-null references",
                new JsonSchema
                {
                    Reference = null
                },
                new JsonSchema
                {
                    Reference = new UriOrFragment("schema/#x")
                },
                false
            },

            new object[]
            {
                "Different minimum array lengths",
                new JsonSchema
                {
                    MinItems = 1
                },
                new JsonSchema
                {
                    MinItems = 2
                },
                false
            },

            // These two schemas would validate the same set of instances, but
            // we consider them unequal because they serialize to different
            // JSON schema strings (the first one does not specify a MaxItems
            // property; the second one does).
            new object[]
            {
                "Missing and zero minimum array lengths",
                new JsonSchema
                {
                },
                new JsonSchema
                {
                    MinItems = 0
                },
                false
            },

            new object[]
            {
                "Different maximum array lengths",
                new JsonSchema
                {
                    MaxItems = 1
                },
                new JsonSchema
                {
                    MaxItems = 2
                },
                false
            },

            // These two schemas would validate the same set of instances, but
            // we consider them unequal because they serialize to different
            // JSON schema strings (the first one does not specify a MaxItems
            // property; the second one does).
            new object[]
            {
                "Missing and zero maximum array lengths",
                new JsonSchema
                {
                },
                new JsonSchema
                {
                    MaxItems = 0
                },
                false
            },

            new object[]
            {
                "Different formats",
                new JsonSchema
                {
                    Format = FormatAttributes.DateTime
                },
                new JsonSchema
                {
                    Format = "email"
                },
                false
            },

            new object[]
            {
                "Null and non-null formats",
                new JsonSchema
                {
                },
                new JsonSchema
                {
                    Format = FormatAttributes.DateTime
                },
                false
            }
        };

        [Theory(DisplayName = "JsonSchema equality tests")]
        [MemberData(nameof(EqualityTestCases))]
        public void EqualityTests(string testName, JsonSchema left, JsonSchema right, bool shouldBeEqual)
        {
            left.Equals(right).Should().Be(shouldBeEqual);
            (left == right).Should().Be(shouldBeEqual);
            (left != right).Should().Be(!shouldBeEqual);
        }
    }
}
