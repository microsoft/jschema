// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Json.Schema.TestUtilities;
using Microsoft.Json.Schema.ToDotNet.UnitTests;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.Json.Schema.ToDotNet.UnitTests.Assert;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public class ClassNameHintTests : CodeGenerationTestBase
    {
        public class TestCase
        {
            public TestCase(
                string name,
                string schemaText,
                string hintedClassName,
                string hintsText,
                string primaryClassText,
                string primaryClassEqualityComparerText,
                string primaryClassComparerText,
                string hintedClassText,
                string hintedClassEqualityComparerText,
                string hintedClassComparerText)
            {
                Name = name;
                SchemaText = schemaText;
                HintedClassName = hintedClassName;
                HintsText = hintsText;
                PrimaryClassText = primaryClassText;
                PrimaryClassEqualityComparerText = primaryClassEqualityComparerText;
                PrimaryClassComparerText = primaryClassComparerText;
                HintedClassText = hintedClassText;
                HintedClassEqualityComparerText = hintedClassEqualityComparerText;
                HintedClassComparerText = hintedClassComparerText;
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
            public string PrimaryClassEqualityComparerText;
            public string PrimaryClassComparerText;
            public string HintedClassText;
            public string HintedClassEqualityComparerText;
            public string HintedClassComparerText;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Name = info.GetValue<string>(nameof(Name));
                SchemaText = info.GetValue<string>(nameof(SchemaText));
                HintedClassName = info.GetValue<string>(nameof(HintedClassName));
                HintsText = info.GetValue<string>(nameof(HintsText));
                PrimaryClassText = info.GetValue<string>(nameof(PrimaryClassText));
                PrimaryClassEqualityComparerText = info.GetValue<string>(nameof(PrimaryClassEqualityComparerText));
                PrimaryClassComparerText = info.GetValue<string>(nameof(PrimaryClassComparerText));
                HintedClassText = info.GetValue<string>(nameof(HintedClassText));
                HintedClassEqualityComparerText = info.GetValue<string>(nameof(HintedClassEqualityComparerText));
                HintedClassComparerText = info.GetValue<string>(nameof(HintedClassComparerText));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Name), Name);
                info.AddValue(nameof(SchemaText), SchemaText);
                info.AddValue(nameof(HintedClassName), HintedClassName);
                info.AddValue(nameof(HintsText), HintsText);
                info.AddValue(nameof(PrimaryClassText), PrimaryClassText);
                info.AddValue(nameof(PrimaryClassEqualityComparerText), PrimaryClassEqualityComparerText);
                info.AddValue(nameof(PrimaryClassComparerText), PrimaryClassComparerText);
                info.AddValue(nameof(HintedClassText), HintedClassText);
                info.AddValue(nameof(HintedClassEqualityComparerText), HintedClassEqualityComparerText);
                info.AddValue(nameof(HintedClassComparerText), HintedClassComparerText);
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
      ""kind"": ""ClassNameHint"",
      ""arguments"": {
        ""className"": ""FileData""
      }
    }
  ]
}",

// PrimaryClassText
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

        public static IComparer<C> Comparer => CComparer.Instance;

        [DataMember(Name = ""file"", IsRequired = false, EmitDefaultValue = false)]
        public FileData File { get; set; }
    }
}",

// PrimaryClassEqualityComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type C for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class CEqualityComparer : IEqualityComparer<C>
    {
        internal static readonly CEqualityComparer Instance = new CEqualityComparer();

        public bool Equals(C left, C right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!FileData.ValueComparer.Equals(left.File, right.File))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(C obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.File != null)
                {
                    result = (result * 31) + obj.File.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}",

// PrimaryClassComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type C for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class CComparer : IComparer<C>
    {
        internal static readonly CComparer Instance = new CComparer();

        public int Compare(C left, C right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = FileDataComparer.Instance.Compare(left.File, right.File);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}",

// HintedClassText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class FileData
    {
        public static IEqualityComparer<FileData> ValueComparer => FileDataEqualityComparer.Instance;

        public bool ValueEquals(FileData other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<FileData> Comparer => FileDataComparer.Instance;

        [DataMember(Name = ""path"", IsRequired = false, EmitDefaultValue = false)]
        public string Path { get; set; }
    }
}",

// HintedClassEqualityComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FileData for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class FileDataEqualityComparer : IEqualityComparer<FileData>
    {
        internal static readonly FileDataEqualityComparer Instance = new FileDataEqualityComparer();

        public bool Equals(FileData left, FileData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Path != right.Path)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(FileData obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Path != null)
                {
                    result = (result * 31) + obj.Path.GetHashCode();
                }
            }

            return result;
        }
    }
}",

// HintedClassComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FileData for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class FileDataComparer : IComparer<FileData>
    {
        internal static readonly FileDataComparer Instance = new FileDataComparer();

        public int Compare(FileData left, FileData right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Path, right.Path);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}"
                ),

            new TestCase(
                "Renamed class has a base class",
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
      ""kind"": ""ClassNameHint"",
      ""arguments"": {
        ""className"": ""FileData""
      }
    }
  ],
  ""fileData"": [
    {
      ""kind"": ""BaseTypeHint"",
      ""arguments"": {
        ""baseTypeNames"": [
          ""PropertyBagHolder""
        ]
      }
    }
  ]
}",

// PrimaryClassText
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

        public static IComparer<C> Comparer => CComparer.Instance;

        [DataMember(Name = ""file"", IsRequired = false, EmitDefaultValue = false)]
        public FileData File { get; set; }
    }
}",

// PrimaryClassEqualityComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type C for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class CEqualityComparer : IEqualityComparer<C>
    {
        internal static readonly CEqualityComparer Instance = new CEqualityComparer();

        public bool Equals(C left, C right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!FileData.ValueComparer.Equals(left.File, right.File))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(C obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.File != null)
                {
                    result = (result * 31) + obj.File.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}",

// PrimaryClassComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type C for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class CComparer : IComparer<C>
    {
        internal static readonly CComparer Instance = new CComparer();

        public int Compare(C left, C right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = FileDataComparer.Instance.Compare(left.File, right.File);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}",

// HintedClassText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class FileData : PropertyBagHolder
    {
        public static IEqualityComparer<FileData> ValueComparer => FileDataEqualityComparer.Instance;

        public bool ValueEquals(FileData other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<FileData> Comparer => FileDataComparer.Instance;

        [DataMember(Name = ""path"", IsRequired = false, EmitDefaultValue = false)]
        public string Path { get; set; }
    }
}",

// HintedClassEqualityComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FileData for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class FileDataEqualityComparer : IEqualityComparer<FileData>
    {
        internal static readonly FileDataEqualityComparer Instance = new FileDataEqualityComparer();

        public bool Equals(FileData left, FileData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Path != right.Path)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(FileData obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Path != null)
                {
                    result = (result * 31) + obj.Path.GetHashCode();
                }
            }

            return result;
        }
    }
}",

// HintedClassComparerText
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FileData for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class FileDataComparer : IComparer<FileData>
    {
        internal static readonly FileDataComparer Instance = new FileDataComparer();

        public int Compare(FileData left, FileData right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Path, right.Path);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}"
                )
        };

        [Theory(DisplayName = nameof(ClassNameHint))]
        [MemberData(nameof(TestCases))]
        public void ClassNameHint(TestCase test)
        {
            Settings.GenerateComparers = true;
            Settings.GenerateEqualityComparers = true;
            Settings.HintDictionary = new HintDictionary(test.HintsText);
            var generator = new DataModelGenerator(Settings, TestFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(test.SchemaText, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [Settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = test.PrimaryClassText,
                    EqualityComparerClassContents = test.PrimaryClassEqualityComparerText,
                    ComparerClassContents = test.PrimaryClassComparerText,
                },
                [test.HintedClassName] = new ExpectedContents
                {
                    ClassContents = test.HintedClassText,
                    EqualityComparerClassContents = test.HintedClassEqualityComparerText,
                    ComparerClassContents = test.HintedClassComparerText,
                }
            };

            Assert.FileContentsMatchExpectedContents(TestFileSystem, expectedContentsDictionary, Settings.GenerateEqualityComparers, Settings.GenerateComparers);
        }
    }
}
