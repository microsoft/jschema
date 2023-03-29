// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class PropertyTypeHintTests : HintTestBase
    {
        public static readonly TheoryData<HintTestCase> TestCases = new TheoryData<HintTestCase>
        {
            new HintTestCase(
                "Specifies property name",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""TheNullableBigIntegerProperty"": {
      ""type"": ""integer""
    },
    ""TheBigIntegerProperty"": {
      ""type"": ""integer"",
      ""default"": ""-1""
    },
    ""TheIntegerProperty"": {
      ""type"": ""integer"",
      ""default"": ""-1"",
    },
    ""TheLongProperty"": {
      ""type"": ""integer"",
      ""default"": ""-1""
    },
    ""TheStringProperty"": {
      ""type"": ""integer"",
      ""default"": ""-1""
    },
    ""TheNullableGuidProperty"": {
      ""type"": ""string""
    },
    ""TheUriProperty"": {
      ""type"": ""string""
    },
    ""TheBoolProperty"": {
      ""type"": ""string"",
      ""default"": true
    },
    ""TheDecimalProperty"": {
      ""type"": ""number"",
      ""default"": ""1.11111""
    },
    ""TheDoubleProperty"": {
      ""type"": ""number"",
      ""default"": ""1.11111""
    },
  }
}",

@"{
  ""C.TheNullableBigIntegerProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""BigInteger""
      }
    }
  ],
  ""C.TheLongProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""Long""
      }
    }
  ],
  ""C.TheStringProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""String""
      }
    }
  ],
  ""C.TheNullableGuidProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""Guid""
      }
    }
  ],
  ""C.TheUriProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""Uri""
      }
    }
  ],
  ""C.TheBoolProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""Bool""
      }
    }
  ],
  ""C.TheDecimalProperty"": [
    {
      ""kind"": ""PropertyTypeHint"",
      ""arguments"": {
        ""typeName"": ""Decimal""
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""TheNullableBigIntegerProperty"", IsRequired = false, EmitDefaultValue = false)]
        public BigInteger? TheNullableBigIntegerProperty { get; set; }
        [DataMember(Name = ""TheBigIntegerProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""-1"")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int TheBigIntegerProperty { get; set; }
        [DataMember(Name = ""TheIntegerProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""-1"")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int TheIntegerProperty { get; set; }
        [DataMember(Name = ""TheLongProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""-1"")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long TheLongProperty { get; set; }
        [DataMember(Name = ""TheStringProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""-1"")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string TheStringProperty { get; set; }
        [DataMember(Name = ""TheNullableGuidProperty"", IsRequired = false, EmitDefaultValue = false)]
        public Guid? TheNullableGuidProperty { get; set; }
        [DataMember(Name = ""TheUriProperty"", IsRequired = false, EmitDefaultValue = false)]
        public Uri TheUriProperty { get; set; }
        [DataMember(Name = ""TheBoolProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool TheBoolProperty { get; set; }
        [DataMember(Name = ""TheDecimalProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""1.11111"")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public decimal TheDecimalProperty { get; set; }
        [DataMember(Name = ""TheDoubleProperty"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""1.11111"")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double TheDoubleProperty { get; set; }
    }
}"
            )
        };

        [Theory(DisplayName = nameof(PropertyNameHint))]
        [MemberData(nameof(TestCases))]
        public void PropertyNameHint(HintTestCase testCase)
        {
            RunHintTestCase(testCase);
        }
    }
}
