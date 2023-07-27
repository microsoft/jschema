// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class PropertyHintNameTests : HintTestBase
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
      ""kind"": ""PropertyHint"",
      ""arguments"": {
        ""name"": ""SchemaUri""
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
            )
        };

        [Theory(DisplayName = nameof(PropertyHintNameTest))]
        [MemberData(nameof(TestCases))]
        public void PropertyHintNameTest(HintTestCase testCase)
        {
            RunHintTestCase(testCase);
        }
    }
}
