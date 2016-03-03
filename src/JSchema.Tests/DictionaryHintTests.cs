// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using Microsoft.JSchema.Generator;
using Xunit;

namespace Microsoft.JSchema.Tests
{
    public class DictionaryHintTests
    {
        private readonly TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public DictionaryHintTests()
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
  ""description"": ""Class with property bag."",
  ""properties"": {
    ""properties"": {
      ""description"": ""Set of key-value pairs."",
      ""type"": ""object""
    }
  }
}",

@"{
  ""C.Properties"": [
    {
      ""$type"": ""Microsoft.JSchema.Generator.DictionaryHint, Microsoft.JSchema""
    }
  ]
}",

@"using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Class with property bag.
    /// </summary>
    public partial class C : IEquatable<C>
    {
        /// <summary>
        /// Set of key-value pairs.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }
        }
    }
}"
            }
        };

        [Theory(DisplayName = "DictionaryHint generates dictionary")]
        [MemberData(nameof(TestCases))]
        public void GeneratesDictionary(
            string schemaText,
            string hintsText,
            string expected)
        {
            JsonSchema schema = SchemaReader.ReadSchema(schemaText);
            _settings.HintDictionary = HintDictionary.Deserialize(hintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            string actual = generator.Generate(schema);

            actual.Should().Be(expected);
        }
    }
}
