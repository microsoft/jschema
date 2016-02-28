// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.JSchema.Generator;
using Xunit;

namespace Microsoft.JSchema.Tests
{
    public class InterfaceHintTests
    {
        private const string PrimaryOutputFilePath = TestFileSystem.OutputDirectory + "\\" + TestSettings.RootClassName + ".cs";

        private readonly TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public InterfaceHintTests()
        {
            _testFileSystem = new TestFileSystem();
            _settings = TestSettings.MakeSettings();
        }

        public static readonly object[] TestCases = new object[]
        {
            new object[]
            {
@"{
  ""type"": ""object"",
  ""description"": ""My class with an interface."",
  ""properties"": {
    ""name"": {
      ""description"": ""The name of this instance."",
      ""type"": ""string""
    }
  },
}",

@"{
  ""c"": [
    {
      ""$type"": ""Microsoft.JSchema.Generator.InterfaceHint, Microsoft.JSchema"",
      ""description"": ""My interface.""
    }
  ]
}",

@"namespace N
{
    /// <summary>
    /// My class with an interface.
    /// </summary>
    public partial class C : IC
    {
        /// <summary>
        /// The name of this instance.
        /// </summary>
        public override string Name { get; set; }
    }
}",

@"namespace N
{
    /// <summary>
    /// My interface.
    /// </summary>
    public interface IC
    {
        /// <summary>
        /// The name of this instance.
        /// </summary>
        string Name { get; set; }
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
            JsonSchema schema = SchemaReader.ReadSchema(schemaText);
            _settings.HintDictionary = HintDictionary.Deserialize(hintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            generator.Generate(schema);

            string interfaceFilePath = TestFileSystem.MakeOutputFilePath("I" + _settings.RootClassName);

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                interfaceFilePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(classText);
            _testFileSystem[interfaceFilePath].Should().Be(interfaceText);
        }
    }
}
