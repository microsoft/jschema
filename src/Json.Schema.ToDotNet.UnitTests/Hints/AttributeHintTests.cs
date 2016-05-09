// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class AttributeHintTests : HintTestBase
    {
        public static readonly TheoryData<HintTestCase> TestCases = new TheoryData<HintTestCase>
        {
            new HintTestCase(
                "No arguments",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""theProperty"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""C.TheProperty"": [
    {
      ""kind"": ""AttributeHint"",
      ""arguments"": {
        ""typeName"": ""Test""
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""theProperty"", IsRequired = false, EmitDefaultValue = false)]
        [Test]
        public int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "One argument",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""theProperty"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""C.TheProperty"": [
    {
      ""kind"": ""AttributeHint"",
      ""arguments"": {
        ""typeName"": ""Test"",
        ""arguments"": [
          ""typeof(string)""
        ]
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""theProperty"", IsRequired = false, EmitDefaultValue = false)]
        [Test(typeof(string))]
        public int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "Multiple arguments",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""theProperty"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""C.TheProperty"": [
    {
      ""kind"": ""AttributeHint"",
      ""arguments"": {
        ""typeName"": ""Test"",
        ""arguments"": [
          ""typeof(string)"",
          ""42"",
          ""\""a\""""
        ]
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""theProperty"", IsRequired = false, EmitDefaultValue = false)]
        [Test(typeof(string), 42, ""a"")]
        public int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "One property",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""theProperty"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""C.TheProperty"": [
    {
      ""kind"": ""AttributeHint"",
      ""arguments"": {
        ""typeName"": ""Test"",
        ""properties"": {
          ""DefaultValueHandling"": ""DefaultValueHandling.Ignore""
        }
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""theProperty"", IsRequired = false, EmitDefaultValue = false)]
        [Test(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "Multiple properties",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""theProperty"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""C.TheProperty"": [
    {
      ""kind"": ""AttributeHint"",
      ""arguments"": {
        ""typeName"": ""Test"",
        ""properties"": {
          ""DefaultValueHandling"": ""DefaultValueHandling.Ignore"",
          ""DefaultValue"": ""42""
        }
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""theProperty"", IsRequired = false, EmitDefaultValue = false)]
        [Test(DefaultValueHandling = DefaultValueHandling.Ignore, DefaultValue = 42)]
        public int TheProperty { get; set; }
    }
}"
            ),
        };

        [Theory(DisplayName = nameof(AttributeHint))]
        [MemberData(nameof(TestCases))]
        public void AttributeHint(HintTestCase testCase)
        {
            RunHintTestCase(testCase);
        }
    }
}
