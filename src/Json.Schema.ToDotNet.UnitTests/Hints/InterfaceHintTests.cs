// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Json.Schema.TestUtilities;
using Microsoft.Json.Schema.ToDotNet.UnitTests;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class InterfaceHintTests : CodeGenerationTestBase
    {
        public static readonly List<object[]> TestCases = new List<object[]>
        {
            // We give the
            new object[]
            {
@"{
  ""type"": ""object"",
  ""description"": ""My class with an interface."",
  ""properties"": {
    ""value"": {
      ""description"": ""The value."",
      ""type"": ""integer""
    },
    ""value2"": {
      ""description"": ""Internal value."",
      ""type"": ""integer""
    }
  }
}",

@"{
  ""c"": [
    {
      ""kind"": ""InterfaceHint"",
      ""arguments"": {
        ""description"": ""My interface.""
      }
    }
  ],
  ""C.Value2"": [
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
    /// <summary>
    /// My class with an interface.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : IC
    {
        /// <summary>
        /// The value.
        /// </summary>
        [DataMember(Name = ""value"", IsRequired = false, EmitDefaultValue = false)]
        public int Value { get; set; }

        /// <summary>
        /// Internal value.
        /// </summary>
        [DataMember(Name = ""value2"", IsRequired = false, EmitDefaultValue = false)]
        internal int Value2 { get; set; }
    }
}",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// My interface.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial interface IC
    {
        /// <summary>
        /// The value.
        /// </summary>
        int Value { get; }
    }
}"
            }
        };

        [Theory(DisplayName = "InterfaceHint generates interfaces in addition to classes")]
        [MemberData(nameof(TestCases))]
        public void GeneratesInterfaceFromClass(
            string schemaText,
            string hintsText,
            string classText,
            string interfaceText)
        {
            Settings.HintDictionary = new HintDictionary(hintsText);
            var generator = new DataModelGenerator(Settings, TestFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            generator.Generate(schema);

            string primaryOutputFilePath = TestFileSystem.MakeOutputFilePath(Settings.RootClassName);
            string interfaceFilePath = TestFileSystem.MakeOutputFilePath("I" + Settings.RootClassName);

            var expectedOutputFiles = new List<string>
            {
                primaryOutputFilePath,
                interfaceFilePath
            };

            TestFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            TestFileSystem.Files.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

            TestFileSystem[primaryOutputFilePath].Should().Be(classText);
            TestFileSystem[interfaceFilePath].Should().Be(interfaceText);
        }
    }
}
