// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.UnitTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class ClassNameHintTests
    {
        private const string PrimaryOutputFilePath = TestFileSystem.OutputDirectory + "\\" + TestSettings.RootClassName + ".cs";

        private readonly TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public ClassNameHintTests()
        {
            _testFileSystem = new TestFileSystem();
            _settings = TestSettings.MakeSettings();
        }

        public class TestCase
        {
            public TestCase(
                string name,
                string schemaText,
                string hintedClassName,
                string hintsText,
                string primaryClassText,
                string hintedClassText)
            {
                Name = name;
                SchemaText = schemaText;
                HintsText = hintsText;
                PrimaryClassText = primaryClassText;
                HintedClassName = hintedClassName;
                HintedClassText = hintedClassText;
            }

            public TestCase()
            {
                // Needed for deserialization.
            }

            public string Name;
            public string SchemaText;
            public string HintedClassName;
            public string HintsText;
            public string PrimaryClassText;
            public string HintedClassText;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                HintedClassName = info.GetValue<string>(nameof(HintedClassName));
                HintsText = info.GetValue<string>(nameof(HintsText));
                PrimaryClassText = info.GetValue<string>(nameof(PrimaryClassText));
                HintedClassText = info.GetValue<string>(nameof(HintedClassText));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(HintedClassName), HintedClassName);
                info.AddValue(nameof(HintsText), HintsText);
                info.AddValue(nameof(PrimaryClassText), PrimaryClassText);
                info.AddValue(nameof(HintedClassText), HintedClassText);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static readonly TheoryData<TestCase> TestCases = new TheoryData<TestCase>
        {
            new TestCase(
                "Change class name",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""file"": {
      ""$ref"": ""#/definitions/file""
    }
  },
  ""definitions"": {
    ""file"": {
      ""type"": ""object"",
      ""properties"": {
        ""path"": {
          ""type"": ""string""
        }
      }
    }
  }
}",

    "FileData",

@"{
  ""file"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.Hints.ClassNameHint, Microsoft.Json.Schema.ToDotNet"",
      ""className"": ""FileData""
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
    public partial class C : IEquatable<C>
    {
        [DataMember(Name = ""file"", IsRequired = false, EmitDefaultValue = false)]
        public FileData File { get; set; }
    }
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class FileData : IEquatable<FileData>
    {
        [DataMember(Name = ""path"", IsRequired = false, EmitDefaultValue = false)]
        public string Path { get; set; }
    }
}")
        };

        [Theory(DisplayName = nameof(ClassNameHint))]
        [MemberData(nameof(TestCases))]
        public void ClassNameHint(TestCase test)
        {
            _settings.HintDictionary = HintDictionary.Deserialize(test.HintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText);

            generator.Generate(schema);

            string hintedFilePath = TestFileSystem.MakeOutputFilePath(test.HintedClassName);

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                hintedFilePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(test.PrimaryClassText);
            _testFileSystem[hintedFilePath].Should().Be(test.HintedClassText);
        }
    }
}
