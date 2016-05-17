// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class PropertyModifiersHintTests : HintTestBase
    {
        public static readonly TheoryData<HintTestCase> TestCases = new TheoryData<HintTestCase>
        {
            new HintTestCase(
                "No modifiers",
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
      ""kind"": ""PropertyModifiersHint"",
      ""arguments"": {
        ""modifiers"": [
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
        int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "One modifier",
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
      ""kind"": ""PropertyModifiersHint"",
      ""arguments"": {
        ""modifiers"": [
          ""internal""
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
        internal int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "Multiple modifiers",
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
      ""kind"": ""PropertyModifiersHint"",
      ""arguments"": {
        ""modifiers"": [
          ""internal"",
          ""override""
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
        internal override int TheProperty { get; set; }
    }
}"
            ),

            new HintTestCase(
                "Invalid modifier",
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
      ""kind"": ""PropertyModifiersHint"",
      ""arguments"": {
        ""modifiers"": [
          ""invalid_modifier""
        ]
      }
    }
  ]
}",

                null,
                true,
                "invalid_modifier"
            ),

            new HintTestCase(
                "Wildcard hint",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""theProperty"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""*.TheProperty"": [
    {
      ""kind"": ""PropertyModifiersHint"",
      ""arguments"": {
        ""modifiers"": [
          ""internal"",
          ""override""
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
        internal override int TheProperty { get; set; }
    }
}"
            ),
        };

        [Theory(DisplayName = nameof(PropertyModifiersHint))]
        [MemberData(nameof(TestCases))]
        public void PropertyModifiersHint(HintTestCase testCase)
        {
            RunHintTestCase(testCase);
        }
    }
}
