// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.UnitTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class EnumHintTests : CodeGenerationTestBase
    {
        public class TestCase : IXunitSerializable
        {
            public TestCase(
                string name,
                bool shouldThrow,
                string schemaText,
                string hintsText,
                string classText,
                string enumFileNameStem,
                string enumText)
            {
                Name = name;
                ShouldThrow = shouldThrow;
                SchemaText = schemaText;
                HintsText = hintsText;
                ClassText = classText;
                EnumFileNameStem = enumFileNameStem;
                EnumText = enumText;
            }

            public TestCase()
            {
                // Needed for deserialization.
            }

            public string Name;
            public bool ShouldThrow;
            public string SchemaText;
            public string HintsText;
            public string ClassText;
            public string EnumFileNameStem;
            public string EnumText;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                ShouldThrow = info.GetValue<bool>(nameof(ShouldThrow));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                HintsText = info.GetValue<string>(nameof(HintsText));
                ClassText = info.GetValue<string>(nameof(ClassText));
                EnumFileNameStem = info.GetValue<string>(nameof(EnumFileNameStem));
                EnumText = info.GetValue<string>(nameof(EnumText));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(ShouldThrow), ShouldThrow);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(HintsText), HintsText);
                info.AddValue(nameof(ClassText), ClassText);
                info.AddValue(nameof(EnumFileNameStem), EnumFileNameStem);
                info.AddValue(nameof(EnumText), EnumText);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static readonly TheoryData<TestCase> TestCases = new TheoryData<TestCase>
        {
            new TestCase(
                "From reference",
                false,
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
      ""kind"": ""EnumHint""
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// My class with an enum.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            ),

            new TestCase(
                "Throws when EnumHint does not specify a type name",
                true,
@"{
  ""type"": ""object"",
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""C.BackgroundColor"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""description"": ""Some pretty colors.""
      }
    }
  ]
}",

                null,
                null,
                null
            ),

            new TestCase(
                "From inline definition",
                false,
@"{
  ""type"": ""object"",
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""C.BackgroundColor"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""typeName"": ""Color"",
        ""description"": ""Some pretty colors.""
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
    /// My class with an enum.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            ),

            new TestCase(
                "Using description from inline definition",
                false,
@"{
  ""type"": ""object"",
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""C.BackgroundColor"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""typeName"": ""Color""
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
    /// My class with an enum.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// The color of the background.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            ),

            new TestCase(
                "Using enumeration constants from hint",
                false,
@"{
  ""type"": ""object"",
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""C.BackgroundColor"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""typeName"": ""Color"",
        ""description"": ""Some pretty colors."",
        ""enumValues"": [ ""crimson"", ""lemon"", ""avocado"" ]
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
    /// My class with an enum.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum Color
    {
        Crimson,
        Lemon,
        Avocado
    }
}"
            ),

            new TestCase(
                "Throws when enum count in hint differs from schema",
                true,
@"{
  ""type"": ""object"",
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""C.BackgroundColor"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""typeName"": ""Color"",
        ""description"": ""Some pretty colors."",
        ""enumValues"": [ ""crimson"", ""lemon"", ""avocado"", ""navy"" ]
      }
    }
  ]
}",
                null,
                null,
                null
            ),

            new TestCase(
                "Specify a 0 value",
                false,
@"{
  ""type"": ""object"",
  ""description"": ""My class with an enum."",
  ""properties"": {
    ""backgroundColor"": {
      ""description"": ""The color of the background."",
      ""enum"": [""red"", ""yellow"", ""green""]
    }
  }
}",

@"{
  ""C.BackgroundColor"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""typeName"": ""Color"",
        ""description"": ""Some pretty colors."",
        ""enumValues"": [ ""crimson"", ""lemon"", ""avocado"" ],
        ""zeroValue"": ""colorless""
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
    /// My class with an enum.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum Color
    {
        Colorless,
        Crimson,
        Lemon,
        Avocado
    }
}"
            )
        };

        [Theory(DisplayName = nameof(EnumHint))]
        [MemberData(nameof(TestCases))]
        public void EnumHint(TestCase test)
        {
            Settings.HintDictionary = new HintDictionary(test.HintsText);
            var generator = new DataModelGenerator(Settings, TestFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText);

            Action action = () => generator.Generate(schema);

            if (test.ShouldThrow)
            {
                action.ShouldThrow<Exception>();
            }
            else
            {
                action();

                string primaryOutputFilePath = TestFileSystem.MakeOutputFilePath(Settings.RootClassName);
                string enumFilePath = TestFileSystem.MakeOutputFilePath(test.EnumFileNameStem);

                var expectedOutputFiles = new List<string>
                {
                    primaryOutputFilePath,
                    enumFilePath
                };

                TestFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
                TestFileSystem.Files.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

                TestFileSystem[primaryOutputFilePath].Should().Be(test.ClassText);
                TestFileSystem[enumFilePath].Should().Be(test.EnumText);
            }
        }
    }
}
