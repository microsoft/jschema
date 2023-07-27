// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class PropertyHintTests : HintTestBase
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
  }
}",

@"{
  ""C.TheNullableBigIntegerProperty"": [
    {
      ""kind"": ""PropertyHint"",
      ""arguments"": {
        ""modifiers"": [
            ""internal"",
            ""override""
        ],
        ""typeName"": ""BigInteger"",
        ""name"": ""OverrideTheNullableBigIntegerProperty""
      }
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Numerics;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""TheNullableBigIntegerProperty"", IsRequired = false, EmitDefaultValue = false)]
        internal override BigInteger? OverrideTheNullableBigIntegerProperty { get; set; }
    }
}"
            )
        };

        [Theory(DisplayName = nameof(PropertyHintTest))]
        [MemberData(nameof(TestCases))]
        public void PropertyHintTest(HintTestCase testCase)
        {
            RunHintTestCase(testCase);
        }
    }
}
