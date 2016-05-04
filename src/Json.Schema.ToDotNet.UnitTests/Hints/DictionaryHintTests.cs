// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.UnitTests;
using Microsoft.Json.Schema.UnitTests;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class DictionaryHintTests : CodeGenerationTestBase
    {
        public static readonly TheoryData<HintTestCase> TestCases = new TheoryData<HintTestCase>
        {
            new HintTestCase(
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
      ""kind"": ""DictionaryHint""
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
    public partial class C
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> DictProp { get; set; }
    }
}"
            ),

            new HintTestCase(
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
      ""kind"": ""DictionaryHint""
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
    public partial class C
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, D> DictProp { get; set; }
    }
}"
            ),

            new HintTestCase(
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
      ""kind"": ""DictionaryHint""
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
    public partial class C
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<D>> DictProp { get; set; }
    }
}"
            ),

            new HintTestCase(
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
      ""kind"": ""DictionaryHint"",
      ""arguments"": {
        ""keyTypeName"": ""Uri""
      }
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
    public partial class C
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<Uri, IList<D>> DictProp { get; set; }
    }
}"
            ),

            new HintTestCase(
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
      ""kind"": ""DictionaryHint""
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
    public partial class C
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = ""dictProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<IList<D>>> DictProp { get; set; }
    }
}"
            )
        };

        [Theory(DisplayName = nameof(DictionaryHint))]
        [MemberData(nameof(TestCases))]
        public void DictionaryHint(HintTestCase test)
        {
            Settings.HintDictionary = new HintDictionary(test.HintsText);
            Settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(Settings, TestFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(test.ExpectedOutput, actual, nameof(DictionaryHint));

            actual.Should().Be(test.ExpectedOutput);
        }
    }
}
