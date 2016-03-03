// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.JSchema.Generator;
using Xunit;

namespace Microsoft.JSchema.Tests
{
    public class EnumHintTests
    {
        private const string PrimaryOutputFilePath = TestFileSystem.OutputDirectory + "\\" + TestSettings.RootClassName + ".cs";

        private readonly TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public EnumHintTests()
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
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""$ref"": ""#/definitions/color""
    }
  },
  ""definitions"": {
    ""color"": {
      ""description"": ""Some pretty colors."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""color"": [
    {
      ""$type"": ""Microsoft.JSchema.Generator.EnumHint, Microsoft.JSchema""
    }
  ]
}",

@"namespace N
{
    /// <summary>
    /// My class with an enum.
    /// </summary>
    public partial class C : IEquatable<C>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        public Color BackgroundColor { get; set; }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }
        }
    }
}",
                "Color",

@"namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            }
        };

        [Theory(DisplayName = "EnumHint generates enumerations")]
        [MemberData(nameof(TestCases))]
        public void GeneratesEnumFromProperty(
            string schemaText,
            string hintsText,
            string classText,
            string enumFileNameStem,
            string enumText)
        {
            JsonSchema schema = SchemaReader.ReadSchema(schemaText);
            _settings.HintDictionary = HintDictionary.Deserialize(hintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            generator.Generate(schema);

            string enumFilePath = TestFileSystem.MakeOutputFilePath(enumFileNameStem);

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                enumFilePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(classText);
            _testFileSystem[enumFilePath].Should().Be(enumText);
        }
    }
}
