// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + BackgroundColor.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (BackgroundColor != other.BackgroundColor)
            {
                return false;
            }

            return true;
        }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            },

            new object[]
            {
                "throws when EnumHint does not specify a type name",
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet"",
      ""description"": ""Some pretty colors.""
    }
  ]
}",

                null,
                null,
                null
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet"",
      ""typeName"": ""Color"",
      ""description"": ""Some pretty colors.""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + BackgroundColor.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (BackgroundColor != other.BackgroundColor)
            {
                return false;
            }

            return true;
        }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet"",
      ""typeName"": ""Color""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + BackgroundColor.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (BackgroundColor != other.BackgroundColor)
            {
                return false;
            }

            return true;
        }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// The color of the background.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public enum Color
    {
        Red,
        Yellow,
        Green
    }
}"
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet"",
      ""typeName"": ""Color"",
      ""description"": ""Some pretty colors."",
      ""enum"": [ ""crimson"", ""lemon"", ""avocado"" ]
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + BackgroundColor.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (BackgroundColor != other.BackgroundColor)
            {
                return false;
            }

            return true;
        }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public enum Color
    {
        Crimson,
        Lemon,
        Avocado
    }
}"
            },

            new object[]
            {
                "throws when enum count in hint differs from schema",
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet"",
      ""typeName"": ""Color"",
      ""description"": ""Some pretty colors."",
      ""enum"": [ ""crimson"", ""lemon"", ""avocado"", ""navy"" ]
    }
  ]
}",
                null,
                null,
                null
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.EnumHint, Microsoft.Json.Schema.ToDotNet"",
      ""typeName"": ""Color"",
      ""description"": ""Some pretty colors."",
      ""enum"": [ ""crimson"", ""lemon"", ""avocado"" ],
      ""zeroValue"": ""colorless""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + BackgroundColor.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (BackgroundColor != other.BackgroundColor)
            {
                return false;
            }

            return true;
        }
    }
}",
                "Color",

@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some pretty colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.7.0.0"")]
    public enum Color
    {
        Colorless,
        Crimson,
        Lemon,
        Avocado
    }
}"
            }
        };

        [Theory(DisplayName = "EnumHint generates enumerations")]
        [MemberData(nameof(TestCases))]
        public void GeneratesEnumFromProperty(
            string testName,
            bool shouldThrow,
            string schemaText,
            string hintsText,
            string classText,
            string enumFileNameStem,
            string enumText)
        {
            _settings.GenerateOverrides = true;
            _settings.HintDictionary = HintDictionary.Deserialize(hintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText);

            Action action = () => generator.Generate(schema);

            if (shouldThrow)
            {
                action.ShouldThrow<Exception>();
            }
            else
            {
                action();

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
}
