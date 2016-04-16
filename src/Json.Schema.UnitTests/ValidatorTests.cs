// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Json.Schema.UnitTests
{
    public class ValidatorTests
    {
        public class TestCase : IXunitSerializable
        {
            public TestCase(
                string name,
                string schemaText,
                string instanceText,
                params string[] expectedMessages)
            {
                Name = name;
                SchemaText = schemaText;
                InstanceText = instanceText;
                ExpectedMessages = expectedMessages;
            }

            public TestCase()
            {
                // Needer for deserializer.
            }

            public string Name;
            public string SchemaText;
            public string InstanceText;
            public string[] ExpectedMessages;

            public override string ToString()
            {
                return Name;
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                InstanceText = info.GetValue<string>(nameof(InstanceText));
                ExpectedMessages = info.GetValue<string[]>(nameof(ExpectedMessages));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(InstanceText), InstanceText);
                info.AddValue(nameof(ExpectedMessages), ExpectedMessages);
            }
        }

        public static TheoryData<TestCase> TestCases = new TheoryData<TestCase>
        {
            new TestCase(
                "Empty schema matches integer",
                "{}",
                "2"
                ),

            new TestCase(
                "Empty schema matches object",
                "{}",
                "{}"
                ),

            new TestCase(
                "Array: maxItems: valid",
                @"{
                  ""type"": ""array"",
                  ""maxItems"": 4
                }",
                "[ 1, 2, 3, 4 ]"
                ),

            new TestCase(
                "Array: maxItems: invalid",
                @"{
                  ""type"": ""array"",
                  ""minItems"": 2,
                  ""maxItems"": 4
                }",
                "[ 1, 2, 3, 4, 5 ]",
                Error.Format(1, 1, ErrorNumber.TooManyArrayItems, 4, 5)
                ),

            new TestCase(
                "Array: minItems: valid",
                @"{
                  ""type"": ""array"",
                  ""minItems"": 2
                }",
                "[ 1, 2 ]"
                ),

            new TestCase(
                "Array: minItems: invalid",
                @"{
                  ""type"": ""array"",
                  ""minItems"": 2
                }",
                "[ 1 ]",
                Error.Format(1, 1, ErrorNumber.TooFewArrayItems, 2, 1)
                ),

            new TestCase(
                "Array: minItems, maxItems: valid without constraint",
                @"{
                  ""type"": ""array"",
                }",
                "[ 1, 2, 3 ]"
                ),

            new TestCase(
                "Array: minItems, maxItems: valid with both constraints",
                @"{
                  ""type"": ""array"",
                  ""minItems"": 2,
                  ""maxItems"": 3,
                }",
                "[ 1, 2, 3 ]"
                ),

            new TestCase(
                "Numeric: maximum: valid integer",
                @"{
                  ""type"": ""integer"",
                  ""maximum"": 2
                }",
                "2"),

            new TestCase(
                "Numeric: maximum: invalid integer",
                @"{
                  ""type"": ""integer"",
                  ""maximum"": 1
                }",
                "2",
                Error.Format(1, 1, ErrorNumber.ValueTooLarge, 2, 1)
                ),

            new TestCase(
                "Numeric: maximum: valid number",
                @"{
                  ""type"": ""number"",
                  ""maximum"": 3.14
                }",
                "3"),

            new TestCase(
                "Numeric: maximum: invalid number",
                @"{
                  ""type"": ""number"",
                  ""maximum"": 3.14
                }",
                "3.2",
                Error.Format(1, 3, ErrorNumber.ValueTooLarge, 3.2, 3.14)
                ),

            new TestCase(
                "Numeric: maximum and exclusiveMaximum: valid integer",
                @"{
                  ""type"": ""integer"",
                  ""maximum"": 2,
                  ""exclusiveMaximum"": true
                }",
                "1"),

            new TestCase(
                "Numeric: maximum and exclusiveMaximum: invalid integer",
                @"{
                  ""type"": ""integer"",
                  ""maximum"": 1,
                  ""exclusiveMaximum"": true
                }",
                "1",
                Error.Format(1, 1, ErrorNumber.ValueTooLargeExclusive, 1, 1)
                ),

            new TestCase(
                "Numeric: maximum and exclusiveMaximum: valid number",
                @"{
                  ""type"": ""number"",
                  ""maximum"": 3.14,
                  ""exclusiveMaximum"": true
                }",
                "3.13"),

            new TestCase(
                "Numeric: maximum and exclusiveMaximum: invalid number",
                @"{
                  ""type"": ""number"",
                  ""maximum"": 3.14,
                  ""exclusiveMaximum"": true
                }",
                "3.14",
                Error.Format(1, 4, ErrorNumber.ValueTooLargeExclusive, 3.14, 3.14)
                ),

            new TestCase(
                "Numeric: minimum: valid integer",
                @"{
                  ""type"": ""integer"",
                  ""minimum"": 2
                }",
                "2"),

            new TestCase(
                "Numeric: minimum: invalid integer",
                @"{
                  ""type"": ""integer"",
                  ""minimum"": 1
                }",
                "0",
                Error.Format(1, 1, ErrorNumber.ValueTooSmall, 0, 1)
                ),

            new TestCase(
                "Numeric: minimum: valid number",
                @"{
                  ""type"": ""number"",
                  ""minimum"": 3.14
                }",
                "4"),

            new TestCase(
                "Numeric: minimum: invalid number",
                @"{
                  ""type"": ""number"",
                  ""minimum"": 3.14
                }",
                "3.13",
                Error.Format(1, 4, ErrorNumber.ValueTooSmall, 3.13, 3.14)
                ),

            new TestCase(
                "Numeric: minimum and exclusiveMinimum: valid integer",
                @"{
                  ""type"": ""integer"",
                  ""minimum"": 2,
                  ""exclusiveMinimum"": true
                }",
                "3"),

            new TestCase(
                "Numeric: minimum and exclusiveMinimum: invalid integer",
                @"{
                  ""type"": ""integer"",
                  ""minimum"": 1,
                  ""exclusiveMinimum"": true
                }",
                "1",
                Error.Format(1, 1, ErrorNumber.ValueTooSmallExclusive, 1, 1)
                ),

            new TestCase(
                "Numeric: maximum and exclusiveMaximum: valid number",
                @"{
                  ""type"": ""number"",
                  ""maximum"": 3.14,
                  ""exclusiveMaximum"": true
                }",
                "3.13"),

            new TestCase(
                "Numeric: minimum and exclusiveMinimum: invalid number",
                @"{
                  ""type"": ""number"",
                  ""minimum"": 3.14,
                  ""exclusiveMinimum"": true
                }",
                "3.14",
                Error.Format(1, 4, ErrorNumber.ValueTooSmallExclusive, 3.14, 3.14)
                ),

            new TestCase(
                "Object: maxProperties: valid",
@"{
  ""type"": ""object"",
  ""maxProperties"": 1,
  ""additionalProperties"": true
}",

@"{
  ""a"": 1
}"),

            new TestCase(
                "Object: maxProperties: invalid",
@"{
  ""type"": ""object"",
  ""maxProperties"": 1,
  ""additionalProperties"": true
}",

@"{
  ""a"": 1,
  ""b"": 2
}",
                Error.Format(1, 1, ErrorNumber.TooManyProperties, 1, 2)
                ),

            new TestCase(
                "Object: minProperties: valid",
@"{
  ""type"": ""object"",
  ""minProperties"": 2,
  ""additionalProperties"": true
}",

@"{
  ""a"": 1,
  ""b"": 2
}"),

            new TestCase(
                "Object: minProperties: invalid",
@"{
  ""type"": ""object"",
  ""minProperties"": 2,
  ""additionalProperties"": true
}",

@"{
  ""a"": 1
}",
                Error.Format(1, 1, ErrorNumber.TooFewProperties, 2, 1)
                ),

            new TestCase(
                "Object: additionalProperties: valid: allowed by Boolean",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""integer""
    }
  },
  ""additionalProperties"": true
}",

@"{
  ""a"": 2,
  ""b"": {}
}"
                ),

            new TestCase(
                "Object: additionalProperties: invalid: disallowed by Boolean",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""integer""
    }
  },
  ""additionalProperties"": false
}",

@"{
  ""a"": 2,
  ""b"": {}
}",
                Error.Format(3, 7, ErrorNumber.AdditionalPropertiesProhibited, "b")
                ),

            new TestCase(
                "Object: additionalProperties: invalid: disallowed by default",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""a"": 2,
  ""b"": {}
}",
                Error.Format(3, 7, ErrorNumber.AdditionalPropertiesProhibited, "b")
                ),

            new TestCase(
                "Object: additionalProperties: valid: allowed by schema",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""integer""
    }
  },
  ""additionalProperties"": {
    ""type"": ""boolean""
  }
}",

@"{
  ""a"": 2,
  ""b"": false
}"
                ),

            new TestCase(
                "Object: additionalProperties: invalid: disallowed by schema",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""integer""
    }
  },
  ""additionalProperties"": {
    ""type"": ""boolean""
  }
}",

@"{
  ""a"": 2,
  ""b"": ""false""
}",
                Error.Format(3, 15, ErrorNumber.WrongType, "b", JTokenType.Boolean, JTokenType.String)
                ),

            new TestCase(
                "Object: additionalProperties: invalid: disallowed by schema on member object",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""object"",
        ""additionalProperties"": {
          ""type"": ""boolean""
        }
      }
    }
  },
}",

@"{
  ""a"": {
    ""x"": 3
  }
}",
                Error.Format(3, 11, ErrorNumber.WrongType, "a.x", JTokenType.Boolean, JTokenType.Integer)
                ),

            new TestCase(
                "Object: required: invalid: missing property on root instance",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""boolean""
    },
    ""b"": {
      ""type"": ""boolean""
    }
  },
  ""required"": [ ""a"", ""b"", ""c"" ]
}",

@"{
    ""b"": true
}",
                Error.Format(1, 1, ErrorNumber.RequiredPropertyMissing, "a"),
                Error.Format(1, 1, ErrorNumber.RequiredPropertyMissing, "c")
                ),

            new TestCase(
                "Object: required: invalid: missing property on member property",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""object"",
      ""properties"": {
        ""x"": {
          ""type"": ""integer""
        },
        ""y"": {
          ""type"": ""integer""
        },
        ""z"": {
          ""type"": ""integer""
        }
      },
      ""required"": [ ""x"", ""y"", ""z"" ]
    }
  }
}",

@"{
  ""a"": {
    ""y"": 2
  }
}",
                Error.Format(2, 9, ErrorNumber.RequiredPropertyMissing, "x"),
                Error.Format(2, 9, ErrorNumber.RequiredPropertyMissing, "z")
                ),

            new TestCase(
                "type: Integer instance matches integer schema",
                @"{ ""type"": ""integer"" }",
                "2"
                ),

            new TestCase(
                "type: Non-integer instance does not match integer schema",
                @"{ ""type"": ""integer"" }",
                "\"s\"",
                Error.Format(1, 3, ErrorNumber.WrongType, Validator.RootObjectName, JTokenType.Integer, JTokenType.String)
                ),

            new TestCase(
                "type: Array instance matches array schema",
                @"{ ""type"": ""array"" }",
                "[]"
                ),

            new TestCase(
                "type: Non-array instance does not match array schema",
                 @"{ ""type"": ""array"" }",
                "true",
                Error.Format(1, 4, ErrorNumber.WrongType, Validator.RootObjectName, JTokenType.Array, JTokenType.Boolean)
                ),

            new TestCase(
                "type: Integer instance matches number schema",
                @"{ ""type"": ""number"" }",
                "2"
                ),

            new TestCase(
                "Object property matches its schema",

@"{
    ""type"": ""object"",
    ""properties"": {
    ""a"": {
        ""type"": ""boolean""
    }
  }
}",

@"{
  ""a"": true
}"
                ),

            new TestCase(
                "Object property does not match its schema",

@"{
  ""type"": ""object"",
  ""properties"": {
    ""a"": {
      ""type"": ""boolean""
    }
  }
}",

@"{
  ""a"": ""true""
}",
                Error.Format(2, 14, ErrorNumber.WrongType, "a", JTokenType.Boolean, JTokenType.String)
                ),
        };

        [Theory(DisplayName = "Validation")]
        [MemberData(nameof(TestCases))]
        public void Tests(TestCase test)
        {
            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText);
            var target = new Validator(schema);
            string[] actualMessages = target.Validate(test.InstanceText);

            actualMessages.Length.Should().Be(test.ExpectedMessages.Length);
            actualMessages.Should().ContainInOrder(test.ExpectedMessages);
        }
    }
}
