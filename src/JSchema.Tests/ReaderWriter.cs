// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.JSchema.Tests
{
    internal static class ReaderWriter
    {
        public static IEnumerable<object[]> TestCases = new[]
        {
            new object[]
            {
                "Empty",
                new JsonSchema()
            },

            new object[]
            {
                "Basic",
                new JsonSchema
                {
                    Id = new UriOrFragment("http://www.example.com/schemas/basic#"),
                    SchemaVersion = JsonSchema.V4Draft,
                    Title = "The title",
                    Description = "The description",
                    Type = JsonType.Object
                }
            },

            new object[]
            {
                "Properties",
                new JsonSchema
                {
                    Type = JsonType.Object,

                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["stringProp"] = new JsonSchema
                        {
                            Type = JsonType.String
                        },

                        ["numberProp"] = new JsonSchema
                        {
                            Type = JsonType.Number
                        },

                        ["booleanProp"] = new JsonSchema
                        {
                            Type = JsonType.Boolean
                        },

                        ["integerProp"] = new JsonSchema
                        {
                            Type = JsonType.Integer
                        },
                    }
                }
            },

            new object[]
            {
                "RequiredProperties",
                new JsonSchema
                {
                    Type = JsonType.Object,

                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["prop1"] = new JsonSchema
                        {
                            Type = JsonType.String
                        },

                        ["prop2"] = new JsonSchema
                        {
                            Type = JsonType.Integer
                        },

                        ["prop3"] = new JsonSchema
                        {
                            Type = JsonType.Boolean
                        }
                    },

                    Required = new[] { "prop1", "prop3" }
                }
            },

            new object[]
            {
                "StringEnum",
                new JsonSchema
                {
                    Type = JsonType.Object,
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["stringEnumProp"] = new JsonSchema
                        {
                            Enum = new object[] { "a", "1" }
                        }
                    }
                }
            },

            new object[]
            {
                "Array",
                new JsonSchema
                {
                    Type = JsonType.Object,
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["arrayProp"] = new JsonSchema
                        {
                            Type = JsonType.Array
                        }
                    }
                }
            },

            new object[]
            {
                "IntegerArray",
                new JsonSchema
                {
                    Type = JsonType.Object,
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["integerArrayProp"] = new JsonSchema
                        {
                            Type = JsonType.Array,
                            Items = new JsonSchema
                            {
                                Type = JsonType.Integer
                            }
                        }
                    }
                }
            },

            new object[]
            {
                "Definitions",
                new JsonSchema
                {
                    Type = JsonType.Object,

                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["rootProp"] = new JsonSchema
                        {
                            Type = JsonType.Boolean
                        }
                    },

                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["def1"] = new JsonSchema
                        {
                            Type = JsonType.Object,

                            Properties = new Dictionary<string, JsonSchema>
                            {
                                ["prop1"] = new JsonSchema
                                {
                                    Type = JsonType.String
                                }
                            }
                        },
                        ["def2"] = new JsonSchema
                        {
                            Type = JsonType.Object,

                            Properties = new Dictionary<string, JsonSchema>
                            {
                                ["prop2"] = new JsonSchema
                                {
                                    Type = JsonType.Integer
                                }
                            }
                        }
                    }
                }
            },

            new object[]
            {
                "Reference",
                new JsonSchema
                {
                    Type = JsonType.Object,

                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["rootProp"] = new JsonSchema
                        {
                            Reference = new UriOrFragment("#/definitions/def1")
                        }
                    },

                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["def1"] = new JsonSchema
                        {
                            Id = new UriOrFragment("def1"),
                            Properties = new Dictionary<string, JsonSchema>
                            {
                                ["prop1"] = new JsonSchema
                                {
                                    Type = JsonType.String
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
