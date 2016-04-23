// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.UnitTests;
using Microsoft.Json.Schema.UnitTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class DictionaryHintTests
    {
        private readonly TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public class TestCase : IXunitSerializable
        {
            public TestCase(
                string name,
                string schemaText,
                string hintsText,
                string expectedOutput)
            {
                Name = name;
                SchemaText = schemaText;
                HintsText = hintsText;
                ExpectedOutput = expectedOutput;
            }

            public TestCase()
            {
                // Required for deserialization.
            }

            public string Name;
            public string SchemaText;
            public string HintsText;
            public string ExpectedOutput;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                HintsText = info.GetValue<string>(nameof(HintsText));
                ExpectedOutput = info.GetValue<string>(nameof(ExpectedOutput));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(HintsText), HintsText);
                info.AddValue(nameof(ExpectedOutput), ExpectedOutput);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public DictionaryHintTests()
        {
            _testFileSystem = new TestFileSystem();
            _settings = TestSettings.MakeSettings();
        }

        public static readonly TheoryData<TestCase> TestCases = new TheoryData<TestCase>
        {
            new TestCase(
                "Dictionary<string, string> (property bag)",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""dictProp"": {
      ""type"": ""object""
    }
  }
}",

@"{
  ""C.DictProp"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.Hints.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : IEquatable<C>
    {
        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> DictProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (DictProp != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in DictProp)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(DictProp, other.DictProp))
            {
                if (DictProp == null || other.DictProp == null || DictProp.Count != other.DictProp.Count)
                {
                    return false;
                }

                foreach (var value_0 in DictProp)
                {
                    string value_1;
                    if (!other.DictProp.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}"
            ),

            new TestCase(
                "Dictionary<string, D>",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""dictProp"": {
      ""type"": ""object"",
      ""additionalProperties"": {
        ""$ref"": ""#/definitions/d""
      }
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""object""
    }
  }
}",

@"{
  ""C.DictProp"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.Hints.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : IEquatable<C>
    {
        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, D> DictProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (DictProp != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in DictProp)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(DictProp, other.DictProp))
            {
                if (DictProp == null || other.DictProp == null || DictProp.Count != other.DictProp.Count)
                {
                    return false;
                }

                foreach (var value_0 in DictProp)
                {
                    D value_1;
                    if (!other.DictProp.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!Object.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}"
            ),

            new TestCase(
                "Dictionary<string, IList<D>>",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""dictProp"": {
      ""type"": ""object"",
      ""additionalProperties"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/definitions/d""
        }
      }
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""object""
    }
  }
}",

@"{
  ""C.DictProp"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.Hints.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : IEquatable<C>
    {
        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<D>> DictProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (DictProp != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in DictProp)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(DictProp, other.DictProp))
            {
                if (DictProp == null || other.DictProp == null || DictProp.Count != other.DictProp.Count)
                {
                    return false;
                }

                foreach (var value_0 in DictProp)
                {
                    IList<D> value_1;
                    if (!other.DictProp.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!Object.ReferenceEquals(value_0.Value, value_1))
                    {
                        if (value_0.Value == null || value_1 == null)
                        {
                            return false;
                        }

                        if (value_0.Value.Count != value_1.Count)
                        {
                            return false;
                        }

                        for (int index_0 = 0; index_0 < value_0.Value.Count; ++index_0)
                        {
                            if (!Object.Equals(value_0.Value[index_0], value_1[index_0]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}"
            ),

            new TestCase(
                "Dictionary<Uri, IList<D>>",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""dictProp"": {
      ""type"": ""object"",
      ""additionalProperties"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/definitions/d""
        }
      }
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""object""
    }
  }
}",

@"{
  ""C.DictProp"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.Hints.DictionaryHint, Microsoft.Json.Schema.ToDotNet"",
      ""KeyTypeName"": ""Uri""
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : IEquatable<C>
    {
        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<Uri, IList<D>> DictProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (DictProp != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in DictProp)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(DictProp, other.DictProp))
            {
                if (DictProp == null || other.DictProp == null || DictProp.Count != other.DictProp.Count)
                {
                    return false;
                }

                foreach (var value_0 in DictProp)
                {
                    IList<D> value_1;
                    if (!other.DictProp.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!Object.ReferenceEquals(value_0.Value, value_1))
                    {
                        if (value_0.Value == null || value_1 == null)
                        {
                            return false;
                        }

                        if (value_0.Value.Count != value_1.Count)
                        {
                            return false;
                        }

                        for (int index_0 = 0; index_0 < value_0.Value.Count; ++index_0)
                        {
                            if (!Object.Equals(value_0.Value[index_0], value_1[index_0]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}"
            ),

            new TestCase(
                "Dictionary<string, IList<IList<D>>>",
@"{
  ""type"": ""object"",
  ""properties"": {
    ""dictProp"": {
      ""type"": ""object"",
      ""additionalProperties"": {
        ""type"": ""array"",
        ""items"": {
          ""type"": ""array"",
          ""items"": {
            ""$ref"": ""#/definitions/d""
          }
        }
      }
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""object""
    }
  }
}",

@"{
  ""C.DictProp"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.Hints.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
    }
  ]
}",

@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : IEquatable<C>
    {
        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<IList<D>>> DictProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (DictProp != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in DictProp)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(DictProp, other.DictProp))
            {
                if (DictProp == null || other.DictProp == null || DictProp.Count != other.DictProp.Count)
                {
                    return false;
                }

                foreach (var value_0 in DictProp)
                {
                    IList<IList<D>> value_1;
                    if (!other.DictProp.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!Object.ReferenceEquals(value_0.Value, value_1))
                    {
                        if (value_0.Value == null || value_1 == null)
                        {
                            return false;
                        }

                        if (value_0.Value.Count != value_1.Count)
                        {
                            return false;
                        }

                        for (int index_0 = 0; index_0 < value_0.Value.Count; ++index_0)
                        {
                            if (!Object.ReferenceEquals(value_0.Value[index_0], value_1[index_0]))
                            {
                                if (value_0.Value[index_0] == null || value_1[index_0] == null)
                                {
                                    return false;
                                }

                                if (value_0.Value[index_0].Count != value_1[index_0].Count)
                                {
                                    return false;
                                }

                                for (int index_1 = 0; index_1 < value_0.Value[index_0].Count; ++index_1)
                                {
                                    if (!Object.Equals(value_0.Value[index_0][index_1], value_1[index_0][index_1]))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}"
            )
        };

        [Theory(DisplayName = nameof(DictionaryHint))]
        [MemberData(nameof(TestCases))]
        public void DictionaryHint(TestCase test)
        {
            _settings.HintDictionary = HintDictionary.Deserialize(test.HintsText);
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(test.ExpectedOutput, actual, nameof(DictionaryHint));

            actual.Should().Be(test.ExpectedOutput);
        }
    }
}
