// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using Microsoft.Json.Schema.UnitTests;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
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
                    var value_1;
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
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
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
                    var value_1;
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
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
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
                    var value_1;
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

                        for (int value_2 = 0; value_2 < value_0.Value.Count; ++value_2)
                        {
                            if (!Object.Equals(value_0.Value[value_2], value_1[value_2]))
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
            },

            new object[]
            {
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
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
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
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
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
                    var value_1;
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

                        for (int value_2 = 0; value_2 < value_0.Value.Count; ++value_2)
                        {
                            if (!Object.ReferenceEquals(value_0.Value[value_2], value_1[value_2]))
                            {
                                if (value_0.Value[value_2] == null || value_1[value_2] == null)
                                {
                                    return false;
                                }

                                if (value_0.Value[value_2].Count != value_1[value_2].Count)
                                {
                                    return false;
                                }

                                for (int value_3 = 0; value_3 < value_0.Value[value_2].Count; ++value_3)
                                {
                                    if (!Object.Equals(value_0.Value[value_2][value_3], value_1[value_2][value_3]))
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
            }
        };

        [Theory(DisplayName = "DictionaryHint generates dictionary")]
        [MemberData(nameof(TestCases))]
        public void GeneratesDictionary(
            string testCaseName,
            string schemaText,
            string hintsText,
            string expected)
        {
            _settings.HintDictionary = HintDictionary.Deserialize(hintsText);
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(expected, actual, nameof(GeneratesDictionary));

            actual.Should().Be(expected);
        }
    }
}
