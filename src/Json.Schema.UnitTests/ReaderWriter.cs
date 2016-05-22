// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.UnitTests
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
                    Type = new JTokenType[] { JTokenType.Object }
                }
            },

            new object[]
            {
                "Properties",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },

                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["stringProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.String }
                        },

                        ["numberProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Float }
                        },

                        ["booleanProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Boolean }
                        },

                        ["integerProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Integer }
                        },
                    }
                }
            },

            new object[]
            {
                "RequiredProperties",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object }
                    ,
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["prop1"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.String }
                        },

                        ["prop2"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Integer }
                        },

                        ["prop3"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Boolean }
                        }
                    },

                    Required = new[] { "prop1", "prop3" }
                }
            },

            new object[]
            {
                "Array",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["arrayProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Array },
                            Items = new Items(
                                new JsonSchema
                                {
                                    Type = new JTokenType[] { JTokenType.Object }
                                }
                            )
                        }
                    },
                    MinItems = 1,
                    MaxItems = null
                }
            },

            new object[]
            {
                "IntegerArray",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["integerArrayProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Array },
                            Items = new Items(
                                new JsonSchema
                                {
                                    Type = new JTokenType[] { JTokenType.Integer }
                                }
                            )
                        }
                    }
                }
            },

            new object[]
            {
                "Definitions",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },

                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["rootProp"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Boolean }
                        }
                    },

                    Definitions = new Dictionary<string, JsonSchema>
                    {
                        ["def1"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Object },

                            Properties = new Dictionary<string, JsonSchema>
                            {
                                ["prop1"] = new JsonSchema
                                {
                                    Type = new JTokenType[] { JTokenType.String }
                                }
                            }
                        },
                        ["def2"] = new JsonSchema
                        {
                            Type = new JTokenType[] { JTokenType.Object },

                            Properties = new Dictionary<string, JsonSchema>
                            {
                                ["prop2"] = new JsonSchema
                                {
                                    Type = new JTokenType[] { JTokenType.Integer }
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
                    Type = new JTokenType[] { JTokenType.Object },

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
                                    Type = new JTokenType[] { JTokenType.String }
                                }
                            }
                        }
                    }
                }
            },

            new object[]
            {
                "DateTime",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },

                    Properties = new Dictionary<string, JsonSchema>
                    {
                         ["startTime"] = new JsonSchema
                         {
                             Type = new JTokenType[] { JTokenType.String },
                             Format = FormatAttributes.DateTime
                         }
                    }
                }
            },

            new object[]
            {
                "AdditionalPropertiesBoolean",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },
                    AdditionalProperties = new AdditionalProperties(true)
                }
            },

            new object[]
            {
                "AdditionalPropertiesSchema",
                new JsonSchema
                {
                    Type = new JTokenType[] { JTokenType.Object },
                    AdditionalProperties = new AdditionalProperties(new JsonSchema
                    {
                        Type = new JTokenType[] { JTokenType.Float }
                    })
                }
            }
        };
    }
}
