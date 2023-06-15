// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class PropertyNameHintTests : HintTestBase
    {
        public static readonly TheoryData<HintTestCase> TestCases = new TheoryData<HintTestCase>
        {
            new HintTestCase(
                "Specifies property name",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""$schema"": {
      ""type"": ""string"",
      ""format"": ""uri""
    }
  }
}",

@"{
  ""C.$schema"": [
    {
      ""kind"": ""PropertyNameHint"",
      ""arguments"": {
        ""dotNetPropertyName"": ""SchemaUri""
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
        [DataMember(Name = ""$schema"", IsRequired = false, EmitDefaultValue = false)]
        public Uri SchemaUri { get; set; }
    }
}"
            ),

            new HintTestCase(
                "Specifies integer property name",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""itemCount"": {
      ""type"": ""integer""
    }
  }
}",

@"{
  ""C.ItemCount"": [
    {
      ""kind"": ""PropertyNameHint"",
      ""arguments"": {
        ""dotNetPropertyName"": ""RenamedItemCount""
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
        [DataMember(Name = ""itemCount"", IsRequired = false, EmitDefaultValue = false)]
        public int? RenamedItemCount { get; set; }
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
