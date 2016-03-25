// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
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
    }
  }
}",

@"{
  ""c"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.InterfaceHint, Microsoft.Json.Schema.ToDotNet"",
      ""description"": ""My interface.""
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
    [DataContract, GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IC, IEquatable<C>
    {
        /// <summary>
        /// The value.
        /// </summary>
        [DataMember(Name = ""value"", IsRequired = false, EmitDefaultValue = false)]
        public int Value { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + Value.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (Value != other.Value)
            {
                return false;
            }

            return true;
        }
    }
}",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// My interface.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public interface IC
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
            _settings.GenerateOverrides = true;
            _settings.HintDictionary = HintDictionary.Deserialize(hintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText);

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
