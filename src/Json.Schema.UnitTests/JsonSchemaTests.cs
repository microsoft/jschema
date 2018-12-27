﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using Microsoft.Json.Schema.TestUtilities;
using Xunit;

namespace Microsoft.Json.Schema.UnitTests
{
    public class JsonSchemaTests
    {
        public static readonly TheoryData<EqualityTestCase> EqualityTestCases = new TheoryData<EqualityTestCase>
        {
            new EqualityTestCase(
                "Empty schemas",
                @"{}",
                @"{}",
                true
            ),

            new EqualityTestCase(
                "All properties equal",
                @"{
                    ""id"": ""http://x/y#"",
                    ""$schema"": ""http://z"",
                    ""title"": ""x"",
                    ""enum"": [ ""a"", ""b"" ],
                    ""items"": {
                      ""type"": ""integer""
                    },
                    ""properties"": {
                      ""a"": {
                        ""type"": ""object""
                      },
                      ""b"": {
                        ""type"": ""string""
                      }
                    },
                    ""patternProperties"": {
                      ""x*"": {
                        ""type"": ""integer""
                      },
                      ""y\\d+"": {
                        ""type"": ""boolean""
                      }
                    },
                    ""required"": [ ""a"" ],
                    ""definitions"": {
                      ""c"": {
                        ""type"": ""integer""
                      },
                      ""d"": {
                        ""type"": ""boolean""
                      }
                    },
                    ""additionalItems"": {
                      ""type"": ""integer""
                    },
                    ""additionalProperties"": true,
                    ""dependencies"": {
                      ""a"": {
                        ""type"": ""string""
                      },
                      ""b"": [ ""c"", ""d"" ]
                    },
                    ""$ref"": ""http://www.example.com/schema/#"",
                    ""maxLength"": 2,
                    ""minLength"": 2,
                    ""pattern"": ""\\d{3}"",
                    ""multipleOf"": 2,
                    ""minItems"": 1,
                    ""maxItems"": 3,
                    ""uniqueItems"": true,
                    ""format"": ""date-time"",
                    ""maximimum"": 2,
                    ""exclusiveMaximum"": false,
                    ""default"": 2,
                }",
                @"{
                    ""id"": ""http://x/y#"",
                    ""$schema"": ""http://z"",
                    ""title"": ""x"",
                    ""enum"": [ ""a"", ""b"" ],
                    ""items"": {
                      ""type"": ""integer""
                    },
                    ""properties"": {
                      ""a"": {
                        ""type"": ""object""
                      },
                      ""b"": {
                        ""type"": ""string""
                      }
                    },
                    ""patternProperties"": {
                      ""x*"": {
                        ""type"": ""integer""
                      },
                      ""y\\d+"": {
                        ""type"": ""boolean""
                      }
                    },
                    ""required"": [ ""a"" ],
                    ""definitions"": {
                      ""c"": {
                        ""type"": ""integer""
                      },
                      ""d"": {
                        ""type"": ""boolean""
                      }
                    },
                    ""additionalItems"": {
                      ""type"": ""integer""
                    },
                    ""additionalProperties"": true,
                    ""dependencies"": {
                      ""a"": {
                        ""type"": ""string""
                      },
                      ""b"": [ ""c"", ""d"" ]
                    },
                    ""$ref"": ""http://www.example.com/schema/#"",
                    ""maxLength"": 2,
                    ""minLength"": 2,
                    ""pattern"": ""\\d{3}"",
                    ""multipleOf"": 2,
                    ""minItems"": 1,
                    ""maxItems"": 3,
                    ""uniqueItems"": true,
                    ""format"": ""date-time"",
                    ""maximimum"": 2,
                    ""exclusiveMaximum"": false,
                    ""default"": 2
                }",
                true
            ),

            new EqualityTestCase(
                "Different Ids",
                @"{
                  ""id"": ""http://x/y#"",
                }",
                @"{
                  ""id"": ""http://x/y#a"",
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null Ids",
                @"{}",
                @"{
                  ""id"": ""http://x/y#"",
                }",
                false
            ),

            new EqualityTestCase(
                "Different schema versions",
                @"{
                  ""$schema"": ""http://z""
                }",
                @"{
                  ""$schema"": ""http://q""
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null schema versions",
                @"{}",
                @"{
                  ""$schema"": ""http://z""
                }",
                false
            ),

            new EqualityTestCase(
                "Different titles",
                @"{
                  ""title"": ""x""
                }",
                @"{
                  ""title"": ""y""
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null titles",
                @"{}",
                @"{
                  ""title"": ""y""
                }",
                false
            ),

            new EqualityTestCase(
                "Different enum lists",
                @"{
                  ""enum"": [ ""a"", ""b"" ]
                }",
                @"{
                  ""enum"": [ ""a"", ""c"" ]
                }",
                false
            ),

            new EqualityTestCase(
                "Same enum lists in different orders",
                @"{
                  ""enum"": [ ""a"", ""b"" ]
                }",
                @"{
                  ""enum"": [ ""b"", ""a"" ]
                }",
                true
            ),

            new EqualityTestCase(
                "Null and non-null enum lists",
                @"{}",
                @"{
                  ""enum"": [ ""a"", ""b"" ]
                }",
                false
            ),

            new EqualityTestCase(
                "Different item schemas",
                @"{
                  ""items"": {
                    ""type"": ""integer""
                  }
                }",
                @"{
                  ""items"": {
                    ""type"": ""boolean""
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null item schemas",
                @"{}",
                @"{
                  ""items"": {
                    ""type"": ""boolean""
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "Different property schemas",
                @"{
                  ""properties"": {
                    ""a"": {
                      ""type"": ""object""
                    },
                    ""b"": {
                      ""type"": ""string""
                    }
                  }
                }",
                @"{
                  ""properties"": {
                    ""a"": {
                      ""type"": ""object""
                    },
                    ""b"": {
                      ""type"": ""number""
                    }
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null property sets",
                @"{}",
                @"{
                  ""properties"": {
                    ""a"": {
                      ""type"": ""object""
                    },
                    ""b"": {
                      ""type"": ""number""
                    }
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "Different required properties",
                @"{
                  ""required"": [ ""a"", ""b"" ]
                }",
                @"{
                  ""required"": [ ""a"" ]
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null required properties",
                @"{}",
                @"{
                  ""required"": [ ""a"" ]
                }",
                false
            ),

            new EqualityTestCase(
                "Different patternProperties",
                @"{
                  ""patternProperties"": {
                    ""x*"": {
                      ""type"": ""integer""
                    },
                    ""y\\d+"": {
                      ""type"": ""boolean""
                    }
                  }
                }",
                @"{
                  ""patternProperties"": {
                    ""\\w"": {
                      ""type"": ""integer""
                    },
                    ""[^ab].+"": {
                      ""type"": ""boolean""
                    }
                  }
                }",
                false
                ),

            new EqualityTestCase(
                "Null and non-null patternProperties",
                @"{
                }",
                @"{
                  ""patternProperties"": {
                    ""x*"": {
                      ""type"": ""integer""
                    },
                    ""y\\d+"": {
                      ""type"": ""boolean""
                    }
                  }
                }",
                false
                ),

            new EqualityTestCase(
                "Different definitions dictionaries",
                @"{
                  ""definitions"": {
                    ""c"": {
                      ""type"": ""integer""
                    },
                    ""d"": {
                      ""type"": ""boolean""
                    }
                  }
                }",
                @"{
                  ""definitions"": {
                    ""e"": {
                      ""type"": ""integer""
                    },
                    ""f"": {
                      ""type"": ""boolean""
                    }
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null definitions dictionaries",
                @"{}",
                @"{
                  ""definitions"": {
                    ""e"": {
                      ""type"": ""integer""
                    },
                    ""f"": {
                      ""type"": ""boolean""
                    }
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "Different references",
                @"{
                  ""$ref"": ""schema/#""
                }",
                @"{
                  ""$ref"": ""schema/#x""
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null references",
                @"{}",
                @"{
                  ""$ref"": ""schema/#x""
                }",
                false
            ),

            new EqualityTestCase(
                "Different minimum array lengths",
                @"{
                  ""minItems"": 1
                }",
                @"{
                  ""minItems"": 2
                }",
                false
            ),

            // These two schemas would validate the same set of instances, but
            // we consider them unequal because they serialize to different
            // JSON schema strings.
            new EqualityTestCase(
                "Missing and zero minimum array lengths",
                @"{}",
                @"{
                  ""minItems"": 0
                }",
                false
            ),

            new EqualityTestCase(
                "Different maximum array lengths",
                @"{
                  ""maxItems"": 1
                }",
                @"{
                  ""maxItems"": 2
                }",
                false
            ),

            new EqualityTestCase(
                "Missing and zero maximum array lengths",
                @"{}",
                @"{
                  ""maxItems"": 0
                }",
                false
            ),

            new EqualityTestCase(
                "Different formats",
                @"{
                  ""format"": ""data-time""
                }",
                @"{
                  ""format"": ""email""
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null formats",
                @"{}",
                @"{
                  ""format"": ""data-time""
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null additionalItems",
                @"{}",
                @"{
                  ""additionalItems"": true
                }",
                false
            ),

            new EqualityTestCase(
                "additionalItems with Boolean value and schema",
                @"{
                  ""additionalItems"": true
                }",
                @"{
                  ""additionalItems"": {}
                }",
                false
            ),

            new EqualityTestCase(
                "additionalItems with different Boolean values",
                @"{
                  ""additionalItems"": true
                }",
                @"{
                  ""additionalItems"": false
                }",
                false
            ),

            new EqualityTestCase(
                "additionalItems with different schemas",
                @"{
                  ""additionalItems"": {
                    ""format"": ""date-time""
                  }
                }",
                @"{
                  ""additionalItems"": {}
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null additionalProperties",
                @"{}",
                @"{
                  ""additionalProperties"": true
                }",
                false
            ),

            new EqualityTestCase(
                "additionalProperties with Boolean value and schema",
                @"{
                  ""additionalProperties"": true
                }",
                @"{
                  ""additionalProperties"": {}
                }",
                false
            ),

            new EqualityTestCase(
                "additionalProperties with different Boolean values",
                @"{
                  ""additionalProperties"": true
                }",
                @"{
                  ""additionalProperties"": false
                }",
                false
            ),

            new EqualityTestCase(
                "additionalProperties with different schemas",
                @"{
                  ""additionalProperties"": {
                    ""format"": ""date-time""
                  }
                }",
                @"{
                  ""additionalProperties"": {}
                }",
                false
            ),

            new EqualityTestCase(
                "Null and non-null dependencies",
                @"{}",
                @"{
                  ""dependencies"": {
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "dependencies with property dependency and schema dependency",
                @"{
                  ""dependencies"": {
                    ""a"": [ ""b"" ]
                  }
                }",
                @"{
                  ""dependencies"": {
                    ""a"": {}
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "dependencies with different property dependencies",
                @"{
                  ""dependencies"": {
                    ""a"": [ ""b"" ]
                  }
                }",
                @"{
                  ""dependencies"": {
                    ""a"": [ ""b"", ""c"" ]
                  }
                }",
                false
            ),

            new EqualityTestCase(
                "dependencies with different schema dependencies",
                @"{
                  ""dependencies"": {
                    ""a"": {
                      ""type"": ""integer""
                    }
                  }
                }",
                @"{
                    ""a"": {
                      ""type"": ""number""
                    }
                }",
                false
            ),

            new EqualityTestCase(
                "Different maxLengths",
                @"{
                  ""maxLength"": 2
                }",
                @"{
                  ""maxLength"": 3
                }",
                false),

            new EqualityTestCase(
                "Null and non-null maxLengths",
                @"{
                  ""maxLength"": 2
                }",
                @"{
                }",
                false),

            new EqualityTestCase(
                "Different minLengths",
                @"{
                  ""minLength"": 2
                }",
                @"{
                  ""minLength"": 3
                }",
                false),

            new EqualityTestCase(
                "Null and non-null minLengths",
                @"{
                  ""minLength"": 2
                }",
                @"{
                }",
                false),

            new EqualityTestCase(
                "Different patterns",
                @"{
                  ""pattern"": ""\\d{3}"",
                }",
                @"{
                  ""pattern"": ""\\d{4}"",
                }",
                false),

            new EqualityTestCase(
                "Null and non-null patterns",
                @"{
                  ""pattern"": ""\\d{3}""
                }",
                @"{
                }",
                false),

            new EqualityTestCase(
                "Different multipleOfs",
                @"{
                  ""multipleOf"": 2
                }",
                @"{
                  ""multipleOf"": 3
                }",
                false),

            new EqualityTestCase(
                "Null and non-null multipleOfs",
                @"{
                  ""multipleOf"": 2
                }",
                @"{
                }",
                false),

            new EqualityTestCase(
                "Different maximums",
                @"{
                  ""maximum"": 1
                }",
                @"{
                  ""maximum"": 2
                }",
                false),

            new EqualityTestCase(
                "Null and non-null maximums",
                @"{
                  ""maximum"": 1
                }",
                @"{
                }",
                false),

            new EqualityTestCase(
                "Different exclusiveMaximums",
                @"{
                  ""maximum"": 1,
                  ""exclusiveMaximum"": true
                }",
                @"{
                  ""maximum"": 1,
                  ""exclusiveMaximum"": false
                }",
                false),

            new EqualityTestCase(
                "True and missing exclusiveMaximums",
                @"{
                  ""maximum"": 1,
                  ""exclusiveMaximum"": true
                }",
                @"{
                  ""maximum"": 1,
                }",
                false),

            new EqualityTestCase(
                "False and missing exclusiveMaximums",
                @"{
                  ""maximum"": 1,
                  ""exclusiveMaximum"": false
                }",
                @"{
                  ""maximum"": 1,
                }",
                false),

            new EqualityTestCase(
                "Different uniqueItems",
                @"{
                  ""uniqueItems"": true
                }",
                @"{
                  ""uniqueItems"": false
                }",
                false
            ),

            // These two schemas would validate the same set of instances, but
            // we consider them unequal because they serialize to different
            // JSON schema strings.
            new EqualityTestCase(
                "False and missing uniqueItems",
                @"{}",
                @"{
                  ""uniqueItems"": false
                }",
                false
            ),

            new EqualityTestCase(
                "Same integer defaults",
                @"{
                    ""default"": 2
                }",
                @"{
                    ""default"": 2
                }",
                true),

            new EqualityTestCase(
                "Different integer defaults",
                @"{
                    ""default"": 2
                }",
                @"{
                    ""default"": 3
                }",
                false),

            new EqualityTestCase(
                "Same string defaults",
                @"{
                    ""default"": ""2""
                }",
                @"{
                    ""default"": ""2""
                }",
                true),

            new EqualityTestCase(
                "Different string defaults",
                @"{
                    ""default"": ""2""
                }",
                @"{
                    ""default"": ""3""
                }",
                false),

            new EqualityTestCase(
                "Same Boolean defaults",
                @"{
                    ""default"": true
                }",
                @"{
                    ""default"": true
                }",
                true),

            new EqualityTestCase(
                "Different Boolean defaults",
                @"{
                    ""default"": false
                }",
                @"{
                    ""default"": true
                }",
                false),

            new EqualityTestCase(
                "Different default types",
                @"{
                    ""default"": 2
                }",
                @"{
                    ""default"": ""2""
                }",
                false),

            new EqualityTestCase(
                "Present and missing defaults",
                @"{
                    ""default"": 2
                }",
                @"{}",
                false)
        };

        [Theory(DisplayName = "JsonSchema equality")]
        [MemberData(nameof(EqualityTestCases))]
        public void EqualityTests(EqualityTestCase test)
        {
            JsonSchema left = SchemaReader.ReadSchema(test.Left, TestUtil.TestFilePath + ".left");
            JsonSchema right = SchemaReader.ReadSchema(test.Right, TestUtil.TestFilePath + ".right");

            left.Equals(right).Should().Be(test.ShouldBeEqual);
            (left == right).Should().Be(test.ShouldBeEqual);
            (left != right).Should().Be(!test.ShouldBeEqual);
        }
    }
}
