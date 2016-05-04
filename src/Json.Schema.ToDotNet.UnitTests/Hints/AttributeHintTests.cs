// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.Hints;
using Microsoft.Json.Schema.UnitTests;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests.Hints
{
    public class AttributeHintTests : CodeGenerationTestBase
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
            )
        };

        [Theory(DisplayName = nameof(AttributeHint))]
        [MemberData(nameof(TestCases))]
        public void DictionaryHint(HintTestCase test)
        {
            Settings.HintDictionary = new HintDictionary(test.HintsText);
            var generator = new DataModelGenerator(Settings, TestFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(test.ExpectedOutput, actual, nameof(DictionaryHint));

            actual.Should().Be(test.ExpectedOutput);
        }
    }
}
