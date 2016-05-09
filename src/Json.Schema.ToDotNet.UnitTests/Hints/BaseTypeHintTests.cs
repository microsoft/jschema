// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class BaseTypeHintTests : HintTestBase
    {
        public static readonly TheoryData<HintTestCase> TestCases = new TheoryData<HintTestCase>
        {
            new HintTestCase(
                "No base types",
@"{
  ""type"": ""object""
}",

@"{
  ""c"": [
    {
      ""kind"": ""BaseTypeHint"",
      ""arguments"": {
        ""baseTypeNames"": [
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
    }
}"
            ),

            new HintTestCase(
                "One base type",
@"{
  ""type"": ""object""
}",

@"{
  ""c"": [
    {
      ""kind"": ""BaseTypeHint"",
      ""arguments"": {
        ""baseTypeNames"": [
          ""B""
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
    public partial class C : B
    {
    }
}"
            ),

            new HintTestCase(
                "Multiple base types",
@"{
  ""type"": ""object""
}",

@"{
  ""c"": [
    {
      ""kind"": ""BaseTypeHint"",
      ""arguments"": {
        ""baseTypeNames"": [
          ""B1"",
          ""B2""
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
    public partial class C : B1, B2
    {
    }
}"
            )
        };

        [Theory(DisplayName = nameof(BaseTypeHint))]
        [MemberData(nameof(TestCases))]
        public void BaseTypeHint(HintTestCase testCase)
        {
            RunHintTestCase(testCase);
        }
    }
}
