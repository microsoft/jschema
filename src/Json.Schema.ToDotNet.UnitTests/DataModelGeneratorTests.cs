// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Json.Schema.TestUtilities;
using Microsoft.Json.Schema.ToDotNet.Hints;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    public class DataModelGeneratorTests
    {
        private static readonly string PrimaryOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName);
        private static readonly string PrimaryEqualityComparerOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName + "EqualityComparer");
        private static readonly string PrimaryComparerOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName + "Comparer");
        private static readonly string ComparerExtensionsOutputFilePath = TestFileSystem.MakeOutputFilePath("ComparerExtensions");
        private static readonly string SyntaxInterfaceOutputFilePath = TestFileSystem.MakeOutputFilePath("ISNode");
        private static readonly string KindEnumOutputFilePath = TestFileSystem.MakeOutputFilePath("SNodeKind");
        private static readonly string RewritingVisitorOutputFilePath = TestFileSystem.MakeOutputFilePath("SRewritingVisitor");

        private TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public DataModelGeneratorTests()
        {
            _testFileSystem = new TestFileSystem();
            _settings = TestSettings.MakeSettings();
        }

        [Fact(DisplayName = "DataModelGenerator throws if output directory exists")]
        public void ThrowsIfOutputDirectoryExists()
        {
            _settings.ForceOverwrite = false;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            // ... and the message should mention the output directory.
            action.Should().Throw<ApplicationException>().WithMessage($"*{TestFileSystem.OutputDirectory}*");
        }

        [Fact(DisplayName = "DataModelGenerator does not throw if output directory does not exist")]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            // Use a directory name other than the default. The mock file system believes
            // that only the default directory exists.
            _settings.OutputDirectory = _settings.OutputDirectory + "x";

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "DataModelGenerator does not throw if ForceOverwrite setting is set")]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            // This is the default from MakeSettings; restated here for explicitness.
            _settings.ForceOverwrite = true;

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "DataModelGenerator throws if root schema is not of type 'object'")]
        public void ThrowsIfRootSchemaIsNotOfTypeObject()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("NotAnObject");

            Action action = () => generator.Generate(schema);

            // ... and the message should mention what the root type actually was.
            action.Should().Throw<ApplicationException>().WithMessage("*number*");
        }

        [Fact(DisplayName = "DataModelGenerator generates class description")]
        public void GeneratesClassDescription()
        {
            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// The description
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
    }
}";
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates properties with built-in types")]
        public void GeneratesPropertiesWithBuiltInTypes()
        {
            const string ExpectedClass =
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

        [DataMember(Name = ""stringProperty"", IsRequired = false, EmitDefaultValue = false)]
        public string StringProperty { get; set; }
        [DataMember(Name = ""numberProperty"", IsRequired = false, EmitDefaultValue = false)]
        public double NumberProperty { get; set; }
        [DataMember(Name = ""booleanProperty"", IsRequired = false, EmitDefaultValue = false)]
        public bool BooleanProperty { get; set; }
        [DataMember(Name = ""integerProperty"", IsRequired = false, EmitDefaultValue = false)]
        public int IntegerProperty { get; set; }
    }
}";

            const string ExpectedEqualityComparerClass =
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

            if (left.StringProperty != right.StringProperty)
            {
                return false;
            }

            if (left.NumberProperty != right.NumberProperty)
            {
                return false;
            }

            if (left.BooleanProperty != right.BooleanProperty)
            {
                return false;
            }

            if (left.IntegerProperty != right.IntegerProperty)
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
                if (obj.StringProperty != null)
                {
                    result = (result * 31) + obj.StringProperty.GetHashCode();
                }

                result = (result * 31) + obj.NumberProperty.GetHashCode();
                result = (result * 31) + obj.BooleanProperty.GetHashCode();
                result = (result * 31) + obj.IntegerProperty.GetHashCode();
            }

            return result;
        }
    }
}";
            const string ExpectedComparerClass =
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

            compareResult = string.Compare(left.StringProperty, right.StringProperty);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.NumberProperty.CompareTo(right.NumberProperty);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.BooleanProperty.CompareTo(right.BooleanProperty);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.IntegerProperty.CompareTo(right.IntegerProperty);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Properties");

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates object-valued property with correct type")]
        public void GeneratesObjectValuedPropertyWithCorrectType()
        {
            const string ExpectedClass =
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

        [DataMember(Name = ""objectProp"", IsRequired = false, EmitDefaultValue = false)]
        public D ObjectProp { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (!D.ValueComparer.Equals(left.ObjectProp, right.ObjectProp))
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
                if (obj.ObjectProp != null)
                {
                    result = (result * 31) + obj.ObjectProp.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = DComparer.Instance.Compare(left.ObjectProp, right.ObjectProp);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Object");

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                },

                ["D"] = new ExpectedContents()
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference is not a fragment")]
        public void ThrowsIfReferenceIsNotAFragment()
        {
            const string SchemaText = @"
{
  ""type"": ""object"",
  ""properties"": {
    ""p"": {
      ""$ref"": ""https://example.com/pschema.schema.json/#""
    }
  },
}";
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*https://example.com/pschema.schema.json/#*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference does not specify a definition")]
        public void ThrowsIfReferenceDoesNotSpecifyADefinition()
        {
            const string SchemaText = @"
{
  ""type"": ""object"",
  ""properties"": {
    ""p"": {
      ""$ref"": ""#/notDefinitions/p""
    }
  },
  ""notDefinitions"": {
    ""p"": {
    }
  }
}";
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*#/notDefinitions/p*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if referenced definition does not exist")]
        public void ThrowsIfReferencedDefinitionDoesNotExist()
        {
            const string SchemaText = @"
{
  ""type"": ""object"",
  ""properties"": {
    ""p"": {
      ""$ref"": ""#/definitions/nonExistentDefinition""
    }
  },
  ""definitions"": {
    ""p"": {
    }
  }
}";
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*nonExistentDefinition*");
        }

        [Fact(DisplayName = "DataModelGenerator generates array-valued properties")]
        public void GeneratesArrayValuedProperties()
        {
            const string ExpectedClass =
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

        [DataMember(Name = ""arrayProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<object> ArrayProp { get; set; }
        [DataMember(Name = ""arrayProp2"", IsRequired = false, EmitDefaultValue = false)]
        public IList<int> ArrayProp2 { get; set; }
        [DataMember(Name = ""arrayProp3"", IsRequired = false, EmitDefaultValue = false)]
        public IList<object> ArrayProp3 { get; set; }
    }
}";

            const string ExpectedEqualityComparerClass =
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

            if (!object.ReferenceEquals(left.ArrayProp, right.ArrayProp))
            {
                if (left.ArrayProp == null || right.ArrayProp == null)
                {
                    return false;
                }

                if (left.ArrayProp.Count != right.ArrayProp.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayProp.Count; ++index_0)
                {
                    if (!object.Equals(left.ArrayProp[index_0], right.ArrayProp[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ArrayProp2, right.ArrayProp2))
            {
                if (left.ArrayProp2 == null || right.ArrayProp2 == null)
                {
                    return false;
                }

                if (left.ArrayProp2.Count != right.ArrayProp2.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.ArrayProp2.Count; ++index_1)
                {
                    if (left.ArrayProp2[index_1] != right.ArrayProp2[index_1])
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ArrayProp3, right.ArrayProp3))
            {
                if (left.ArrayProp3 == null || right.ArrayProp3 == null)
                {
                    return false;
                }

                if (left.ArrayProp3.Count != right.ArrayProp3.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.ArrayProp3.Count; ++index_2)
                {
                    if (!object.Equals(left.ArrayProp3[index_2], right.ArrayProp3[index_2]))
                    {
                        return false;
                    }
                }
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
                if (obj.ArrayProp != null)
                {
                    foreach (var value_0 in obj.ArrayProp)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }

                if (obj.ArrayProp2 != null)
                {
                    foreach (var value_1 in obj.ArrayProp2)
                    {
                        result = result * 31;
                        result = (result * 31) + value_1.GetHashCode();
                    }
                }

                if (obj.ArrayProp3 != null)
                {
                    foreach (var value_2 in obj.ArrayProp3)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}";
            const string ExpectedComparerClass =
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

            compareResult = left.ArrayProp.ListCompares(right.ArrayProp);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ArrayProp2.ListCompares(right.ArrayProp2);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ArrayProp3.ListCompares(right.ArrayProp3);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Array");

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayValuedProperties));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties")]
        public void GeneratesXmlCommentsForProperties()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// An example property.
        /// </summary>
        [DataMember(Name = ""exampleProp"", IsRequired = false, EmitDefaultValue = false)]
        public string ExampleProp { get; set; }
    }
}";

            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties whose property type is ref")]
        public void GeneratesXmlCommentsForPropertiesWhosePropertyTypeIsRef()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""description"": ""Describes a console window."",
  ""properties"": {
    ""foregroundColor"": {
      ""$ref"": ""#/definitions/color"",
      ""description"": ""The color of the text on the screen.""
    },
    ""backgroundColor"": {
      ""$ref"": ""#/definitions/color"",
      ""description"": ""The color of the screen background.""
    },
  },
  ""definitions"": {
    ""color"": {
      ""type"": ""object"",
      ""description"": ""Describes a color with R, G, and B components."",
      ""properties"": {
        ""red"": {
          ""type"": ""integer"",
          ""description"": ""The value of the R component.""
        },
        ""green"": {
          ""type"": ""integer"",
          ""description"": ""The value of the G component.""
        },
        ""blue"": {
          ""type"": ""integer"",
          ""description"": ""The value of the B component.""
        }
      }
    }
  }
}";

            const string ExpectedRootClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// Describes a console window.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class ConsoleWindow
    {
        public static IEqualityComparer<ConsoleWindow> ValueComparer => ConsoleWindowEqualityComparer.Instance;

        public bool ValueEquals(ConsoleWindow other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<ConsoleWindow> Comparer => ConsoleWindowComparer.Instance;

        /// <summary>
        /// The color of the text on the screen.
        /// </summary>
        [DataMember(Name = ""foregroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color ForegroundColor { get; set; }

        /// <summary>
        /// The color of the screen background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}";
            const string ExpectedRootEqualityComparerClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ConsoleWindow for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class ConsoleWindowEqualityComparer : IEqualityComparer<ConsoleWindow>
    {
        internal static readonly ConsoleWindowEqualityComparer Instance = new ConsoleWindowEqualityComparer();

        public bool Equals(ConsoleWindow left, ConsoleWindow right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Color.ValueComparer.Equals(left.ForegroundColor, right.ForegroundColor))
            {
                return false;
            }

            if (!Color.ValueComparer.Equals(left.BackgroundColor, right.BackgroundColor))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(ConsoleWindow obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.ForegroundColor != null)
                {
                    result = (result * 31) + obj.ForegroundColor.ValueGetHashCode();
                }

                if (obj.BackgroundColor != null)
                {
                    result = (result * 31) + obj.BackgroundColor.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}";

            const string ExpectedRootComparerClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ConsoleWindow for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class ConsoleWindowComparer : IComparer<ConsoleWindow>
    {
        internal static readonly ConsoleWindowComparer Instance = new ConsoleWindowComparer();

        public int Compare(ConsoleWindow left, ConsoleWindow right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = ColorComparer.Instance.Compare(left.ForegroundColor, right.ForegroundColor);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ColorComparer.Instance.Compare(left.BackgroundColor, right.BackgroundColor);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            const string ExpectedColorClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// Describes a color with R, G, and B components.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class Color
    {
        public static IEqualityComparer<Color> ValueComparer => ColorEqualityComparer.Instance;

        public bool ValueEquals(Color other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<Color> Comparer => ColorComparer.Instance;

        /// <summary>
        /// The value of the R component.
        /// </summary>
        [DataMember(Name = ""red"", IsRequired = false, EmitDefaultValue = false)]
        public int Red { get; set; }

        /// <summary>
        /// The value of the G component.
        /// </summary>
        [DataMember(Name = ""green"", IsRequired = false, EmitDefaultValue = false)]
        public int Green { get; set; }

        /// <summary>
        /// The value of the B component.
        /// </summary>
        [DataMember(Name = ""blue"", IsRequired = false, EmitDefaultValue = false)]
        public int Blue { get; set; }
    }
}";

            const string ExpectedColorEqualityComparerClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Color for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class ColorEqualityComparer : IEqualityComparer<Color>
    {
        internal static readonly ColorEqualityComparer Instance = new ColorEqualityComparer();

        public bool Equals(Color left, Color right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Red != right.Red)
            {
                return false;
            }

            if (left.Green != right.Green)
            {
                return false;
            }

            if (left.Blue != right.Blue)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Color obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Red.GetHashCode();
                result = (result * 31) + obj.Green.GetHashCode();
                result = (result * 31) + obj.Blue.GetHashCode();
            }

            return result;
        }
    }
}";
            const string ExpectedColorComparerClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Color for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class ColorComparer : IComparer<Color>
    {
        internal static readonly ColorComparer Instance = new ColorComparer();

        public int Compare(Color left, Color right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.Red.CompareTo(right.Red);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Green.CompareTo(right.Green);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Blue.CompareTo(right.Blue);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            _settings.RootClassName = "ConsoleWindow";
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedRootClass,
                    EqualityComparerClassContents = ExpectedRootEqualityComparerClass,
                    ComparerClassContents = ExpectedRootComparerClass
                },
                ["Color"] = new ExpectedContents
                {
                    ClassContents = ExpectedColorClass,
                    EqualityComparerClassContents = ExpectedColorEqualityComparerClass,
                    ComparerClassContents = ExpectedColorComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates copyright notice")]
        public void GeneratesCopyrightAtTopOfFile()
        {
            _settings.CopyrightNotice =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.

";
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.

using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// An example property.
        /// </summary>
        [DataMember(Name = ""exampleProp"", IsRequired = false, EmitDefaultValue = false)]
        public string ExampleProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates cloning code")]
        public void GeneratesCloningCode()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""integerProperty"": {
      ""type"": ""integer"",
      ""description"": ""An integer property.""
    },
    ""integerPropertyWithDefault"": {
      ""type"": ""integer"",
      ""description"": ""An integer property with a default value."",
      ""default"": 42
    },
    ""numberProperty"": {
      ""type"": ""number"",
      ""description"": ""A number property.""
    },
    ""numberPropertyWithDefault"": {
      ""type"": ""number"",
      ""description"": ""A number property with a default value."",
      ""default"": 42.1
    },
    ""stringProperty"": {
      ""type"": ""string"",
      ""description"": ""A string property.""
    },
    ""stringPropertyWithDefault"": {
      ""type"": ""string"",
      ""description"": ""A string property with a default value."",
      ""default"": ""Don't panic.""
    },
    ""booleanProperty"": {
      ""type"": ""boolean"",
      ""description"": ""A Boolean property.""
    },
    ""booleanPropertyWithTrueDefault"": {
      ""type"": ""boolean"",
      ""description"": ""A Boolean property with a true default value."",
      ""default"": true
    },
    ""booleanPropertyWithFalseDefault"": {
      ""type"": ""boolean"",
      ""description"": ""A Boolean property with a false default value."",
      ""default"": false
    },
    ""enumeratedPropertyWithDefault"": {
      ""description"": ""An enumerated property with a default value."",
      ""enum"": [ ""red"", ""green"", ""blue"", ""black"", ""white"" ],
      ""default"": ""green""
    },
    ""arrayProp"": {
      ""type"": ""array"",
      ""description"": ""An array property."",
      ""items"": {
        ""type"": ""number""
      }
    },
    ""uriProp"": {
      ""type"": ""string"",
      ""description"": ""A Uri property."",
      ""format"": ""uri""
    },
    ""dateTimeProp"": {
      ""type"": ""string"",
      ""description"": ""A DateTime property."",
      ""format"": ""date-time""
    },
    ""referencedTypeProp"": {
      ""$ref"": ""#/definitions/d""
    },
    ""arrayOfRefProp"": {
      ""type"": ""array"",
      ""description"": ""An array of a cloneable type."",
      ""items"": {
        ""$ref"": ""#/definitions/d""
      }
    },
    ""arrayOfArrayProp"": {
      ""type"": ""array"",
      ""description"": ""An array of arrays."",
      ""items"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/definitions/d""
        }
      }
    },
    ""dictionaryProp"": {
      ""description"": ""A dictionary property."",
      ""type"": ""object""
    },
    ""dictionaryWithPrimitiveSchemaProp"": {
      ""description"": ""A dictionary property whose values are defined by a primitive additionalProperties schema."",
      ""type"": ""object"",
      ""additionalProperties"": {
        ""type"": ""number""
      }
    },
    ""dictionaryWithObjectSchemaProp"": {
      ""description"": ""A dictionary property whose values are defined by an object-valued additionalProperties schema."",
      ""type"": ""object"",
      ""additionalProperties"": {
        ""$ref"": ""#/definitions/d""
      }
    },
    ""dictionaryWithObjectArraySchemaProp"": {
      ""description"": ""A dictionary property whose values are defined by an array-of-object-valued additionalProperties schema."",
      ""type"": ""object"",
      ""additionalProperties"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/definitions/d""
        }
      }
    },
    ""dictionaryWithUriKeyProp"": {
      ""description"": ""A dictionary property whose keys are Uris."",
      ""type"": ""object"",
      ""additionalProperties"": {
        ""$ref"": ""#/definitions/d""
      }
    },
    ""dictionaryWithHintedValueProp"": {
      ""description"": ""A dictionary property whose value type is hinted."",
      ""type"": ""object""
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""object""
    }
  }
}", TestUtil.TestFilePath);

            const string HintsText =
@"{
  ""C.EnumeratedPropertyWithDefault"": [
    {
      ""kind"": ""EnumHint"",
      ""arguments"": {
        ""typeName"": ""Color"",
        ""description"": ""Some colors.""
      }
    }
  ],
  ""C.DictionaryProp"": [
    {
      ""kind"": ""DictionaryHint""
    }
  ],
  ""C.DictionaryWithPrimitiveSchemaProp"": [
    {
      ""kind"": ""DictionaryHint""
    }
  ],
  ""C.DictionaryWithObjectSchemaProp"": [
    {
      ""kind"": ""DictionaryHint""
    }
  ],
  ""C.DictionaryWithObjectArraySchemaProp"": [
    {
      ""kind"": ""DictionaryHint""
    }
  ],
  ""C.DictionaryWithUriKeyProp"": [
    {
      ""kind"": ""DictionaryHint"",
      ""arguments"": {
        ""keyTypeName"": ""Uri""
      }
    }
  ],
  ""C.DictionaryWithHintedValueProp"": [
    {
      ""kind"": ""DictionaryHint"",
      ""arguments"": {
        ""valueTypeName"": ""V"",
        ""comparisonKind"": ""ObjectEquals"",
        ""hashKind"": ""ScalarReferenceType"",
        ""initializationKind"": ""SimpleAssign""
      }
    }
  ]
}";

            const string ExpectedClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        public SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

        /// <summary>
        /// An integer property.
        /// </summary>
        [DataMember(Name = ""integerProperty"", IsRequired = false, EmitDefaultValue = false)]
        public int IntegerProperty { get; set; }

        /// <summary>
        /// An integer property with a default value.
        /// </summary>
        [DataMember(Name = ""integerPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(42)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int IntegerPropertyWithDefault { get; set; }

        /// <summary>
        /// A number property.
        /// </summary>
        [DataMember(Name = ""numberProperty"", IsRequired = false, EmitDefaultValue = false)]
        public double NumberProperty { get; set; }

        /// <summary>
        /// A number property with a default value.
        /// </summary>
        [DataMember(Name = ""numberPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(42.1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double NumberPropertyWithDefault { get; set; }

        /// <summary>
        /// A string property.
        /// </summary>
        [DataMember(Name = ""stringProperty"", IsRequired = false, EmitDefaultValue = false)]
        public string StringProperty { get; set; }

        /// <summary>
        /// A string property with a default value.
        /// </summary>
        [DataMember(Name = ""stringPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""Don't panic."")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StringPropertyWithDefault { get; set; }

        /// <summary>
        /// A Boolean property.
        /// </summary>
        [DataMember(Name = ""booleanProperty"", IsRequired = false, EmitDefaultValue = false)]
        public bool BooleanProperty { get; set; }

        /// <summary>
        /// A Boolean property with a true default value.
        /// </summary>
        [DataMember(Name = ""booleanPropertyWithTrueDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool BooleanPropertyWithTrueDefault { get; set; }

        /// <summary>
        /// A Boolean property with a false default value.
        /// </summary>
        [DataMember(Name = ""booleanPropertyWithFalseDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool BooleanPropertyWithFalseDefault { get; set; }

        /// <summary>
        /// An enumerated property with a default value.
        /// </summary>
        [DataMember(Name = ""enumeratedPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(Color.Green)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Color EnumeratedPropertyWithDefault { get; set; }

        /// <summary>
        /// An array property.
        /// </summary>
        [DataMember(Name = ""arrayProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<double> ArrayProp { get; set; }

        /// <summary>
        /// A Uri property.
        /// </summary>
        [DataMember(Name = ""uriProp"", IsRequired = false, EmitDefaultValue = false)]
        public Uri UriProp { get; set; }

        /// <summary>
        /// A DateTime property.
        /// </summary>
        [DataMember(Name = ""dateTimeProp"", IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTimeProp { get; set; }
        [DataMember(Name = ""referencedTypeProp"", IsRequired = false, EmitDefaultValue = false)]
        public D ReferencedTypeProp { get; set; }

        /// <summary>
        /// An array of a cloneable type.
        /// </summary>
        [DataMember(Name = ""arrayOfRefProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<D> ArrayOfRefProp { get; set; }

        /// <summary>
        /// An array of arrays.
        /// </summary>
        [DataMember(Name = ""arrayOfArrayProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<D>> ArrayOfArrayProp { get; set; }

        /// <summary>
        /// A dictionary property.
        /// </summary>
        [DataMember(Name = ""dictionaryProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> DictionaryProp { get; set; }

        /// <summary>
        /// A dictionary property whose values are defined by a primitive additionalProperties schema.
        /// </summary>
        [DataMember(Name = ""dictionaryWithPrimitiveSchemaProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, double> DictionaryWithPrimitiveSchemaProp { get; set; }

        /// <summary>
        /// A dictionary property whose values are defined by an object-valued additionalProperties schema.
        /// </summary>
        [DataMember(Name = ""dictionaryWithObjectSchemaProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, D> DictionaryWithObjectSchemaProp { get; set; }

        /// <summary>
        /// A dictionary property whose values are defined by an array-of-object-valued additionalProperties schema.
        /// </summary>
        [DataMember(Name = ""dictionaryWithObjectArraySchemaProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<D>> DictionaryWithObjectArraySchemaProp { get; set; }

        /// <summary>
        /// A dictionary property whose keys are Uris.
        /// </summary>
        [DataMember(Name = ""dictionaryWithUriKeyProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<Uri, D> DictionaryWithUriKeyProp { get; set; }

        /// <summary>
        /// A dictionary property whose value type is hinted.
        /// </summary>
        [DataMember(Name = ""dictionaryWithHintedValueProp"", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, V> DictionaryWithHintedValueProp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class.
        /// </summary>
        public C()
        {
            IntegerPropertyWithDefault = 42;
            NumberPropertyWithDefault = 42.1;
            StringPropertyWithDefault = ""Don't panic."";
            BooleanPropertyWithTrueDefault = true;
            BooleanPropertyWithFalseDefault = false;
            EnumeratedPropertyWithDefault = Color.Green;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the supplied values.
        /// </summary>
        /// <param name=""integerProperty"">
        /// An initialization value for the <see cref=""P:IntegerProperty"" /> property.
        /// </param>
        /// <param name=""integerPropertyWithDefault"">
        /// An initialization value for the <see cref=""P:IntegerPropertyWithDefault"" /> property.
        /// </param>
        /// <param name=""numberProperty"">
        /// An initialization value for the <see cref=""P:NumberProperty"" /> property.
        /// </param>
        /// <param name=""numberPropertyWithDefault"">
        /// An initialization value for the <see cref=""P:NumberPropertyWithDefault"" /> property.
        /// </param>
        /// <param name=""stringProperty"">
        /// An initialization value for the <see cref=""P:StringProperty"" /> property.
        /// </param>
        /// <param name=""stringPropertyWithDefault"">
        /// An initialization value for the <see cref=""P:StringPropertyWithDefault"" /> property.
        /// </param>
        /// <param name=""booleanProperty"">
        /// An initialization value for the <see cref=""P:BooleanProperty"" /> property.
        /// </param>
        /// <param name=""booleanPropertyWithTrueDefault"">
        /// An initialization value for the <see cref=""P:BooleanPropertyWithTrueDefault"" /> property.
        /// </param>
        /// <param name=""booleanPropertyWithFalseDefault"">
        /// An initialization value for the <see cref=""P:BooleanPropertyWithFalseDefault"" /> property.
        /// </param>
        /// <param name=""enumeratedPropertyWithDefault"">
        /// An initialization value for the <see cref=""P:EnumeratedPropertyWithDefault"" /> property.
        /// </param>
        /// <param name=""arrayProp"">
        /// An initialization value for the <see cref=""P:ArrayProp"" /> property.
        /// </param>
        /// <param name=""uriProp"">
        /// An initialization value for the <see cref=""P:UriProp"" /> property.
        /// </param>
        /// <param name=""dateTimeProp"">
        /// An initialization value for the <see cref=""P:DateTimeProp"" /> property.
        /// </param>
        /// <param name=""referencedTypeProp"">
        /// An initialization value for the <see cref=""P:ReferencedTypeProp"" /> property.
        /// </param>
        /// <param name=""arrayOfRefProp"">
        /// An initialization value for the <see cref=""P:ArrayOfRefProp"" /> property.
        /// </param>
        /// <param name=""arrayOfArrayProp"">
        /// An initialization value for the <see cref=""P:ArrayOfArrayProp"" /> property.
        /// </param>
        /// <param name=""dictionaryProp"">
        /// An initialization value for the <see cref=""P:DictionaryProp"" /> property.
        /// </param>
        /// <param name=""dictionaryWithPrimitiveSchemaProp"">
        /// An initialization value for the <see cref=""P:DictionaryWithPrimitiveSchemaProp"" /> property.
        /// </param>
        /// <param name=""dictionaryWithObjectSchemaProp"">
        /// An initialization value for the <see cref=""P:DictionaryWithObjectSchemaProp"" /> property.
        /// </param>
        /// <param name=""dictionaryWithObjectArraySchemaProp"">
        /// An initialization value for the <see cref=""P:DictionaryWithObjectArraySchemaProp"" /> property.
        /// </param>
        /// <param name=""dictionaryWithUriKeyProp"">
        /// An initialization value for the <see cref=""P:DictionaryWithUriKeyProp"" /> property.
        /// </param>
        /// <param name=""dictionaryWithHintedValueProp"">
        /// An initialization value for the <see cref=""P:DictionaryWithHintedValueProp"" /> property.
        /// </param>
        public C(int integerProperty, int integerPropertyWithDefault, double numberProperty, double numberPropertyWithDefault, string stringProperty, string stringPropertyWithDefault, bool booleanProperty, bool booleanPropertyWithTrueDefault, bool booleanPropertyWithFalseDefault, Color enumeratedPropertyWithDefault, IEnumerable<double> arrayProp, Uri uriProp, DateTime dateTimeProp, D referencedTypeProp, IEnumerable<D> arrayOfRefProp, IEnumerable<IEnumerable<D>> arrayOfArrayProp, IDictionary<string, string> dictionaryProp, IDictionary<string, double> dictionaryWithPrimitiveSchemaProp, IDictionary<string, D> dictionaryWithObjectSchemaProp, IDictionary<string, IList<D>> dictionaryWithObjectArraySchemaProp, IDictionary<Uri, D> dictionaryWithUriKeyProp, IDictionary<string, V> dictionaryWithHintedValueProp)
        {
            Init(integerProperty, integerPropertyWithDefault, numberProperty, numberPropertyWithDefault, stringProperty, stringPropertyWithDefault, booleanProperty, booleanPropertyWithTrueDefault, booleanPropertyWithFalseDefault, enumeratedPropertyWithDefault, arrayProp, uriProp, dateTimeProp, referencedTypeProp, arrayOfRefProp, arrayOfArrayProp, dictionaryProp, dictionaryWithPrimitiveSchemaProp, dictionaryWithObjectSchemaProp, dictionaryWithObjectArraySchemaProp, dictionaryWithUriKeyProp, dictionaryWithHintedValueProp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the specified instance.
        /// </summary>
        /// <param name=""other"">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref=""ArgumentNullException"">
        /// Thrown if <paramref name=""other"" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.IntegerProperty, other.IntegerPropertyWithDefault, other.NumberProperty, other.NumberPropertyWithDefault, other.StringProperty, other.StringPropertyWithDefault, other.BooleanProperty, other.BooleanPropertyWithTrueDefault, other.BooleanPropertyWithFalseDefault, other.EnumeratedPropertyWithDefault, other.ArrayProp, other.UriProp, other.DateTimeProp, other.ReferencedTypeProp, other.ArrayOfRefProp, other.ArrayOfArrayProp, other.DictionaryProp, other.DictionaryWithPrimitiveSchemaProp, other.DictionaryWithObjectSchemaProp, other.DictionaryWithObjectArraySchemaProp, other.DictionaryWithUriKeyProp, other.DictionaryWithHintedValueProp);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        private void Init(int integerProperty, int integerPropertyWithDefault, double numberProperty, double numberPropertyWithDefault, string stringProperty, string stringPropertyWithDefault, bool booleanProperty, bool booleanPropertyWithTrueDefault, bool booleanPropertyWithFalseDefault, Color enumeratedPropertyWithDefault, IEnumerable<double> arrayProp, Uri uriProp, DateTime dateTimeProp, D referencedTypeProp, IEnumerable<D> arrayOfRefProp, IEnumerable<IEnumerable<D>> arrayOfArrayProp, IDictionary<string, string> dictionaryProp, IDictionary<string, double> dictionaryWithPrimitiveSchemaProp, IDictionary<string, D> dictionaryWithObjectSchemaProp, IDictionary<string, IList<D>> dictionaryWithObjectArraySchemaProp, IDictionary<Uri, D> dictionaryWithUriKeyProp, IDictionary<string, V> dictionaryWithHintedValueProp)
        {
            IntegerProperty = integerProperty;
            IntegerPropertyWithDefault = integerPropertyWithDefault;
            NumberProperty = numberProperty;
            NumberPropertyWithDefault = numberPropertyWithDefault;
            StringProperty = stringProperty;
            StringPropertyWithDefault = stringPropertyWithDefault;
            BooleanProperty = booleanProperty;
            BooleanPropertyWithTrueDefault = booleanPropertyWithTrueDefault;
            BooleanPropertyWithFalseDefault = booleanPropertyWithFalseDefault;
            EnumeratedPropertyWithDefault = enumeratedPropertyWithDefault;
            if (arrayProp != null)
            {
                var destination_0 = new List<double>();
                foreach (var value_0 in arrayProp)
                {
                    destination_0.Add(value_0);
                }

                ArrayProp = destination_0;
            }

            if (uriProp != null)
            {
                UriProp = new Uri(uriProp.OriginalString, uriProp.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            DateTimeProp = dateTimeProp;
            if (referencedTypeProp != null)
            {
                ReferencedTypeProp = new D(referencedTypeProp);
            }

            if (arrayOfRefProp != null)
            {
                var destination_1 = new List<D>();
                foreach (var value_1 in arrayOfRefProp)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new D(value_1));
                    }
                }

                ArrayOfRefProp = destination_1;
            }

            if (arrayOfArrayProp != null)
            {
                var destination_2 = new List<IList<D>>();
                foreach (var value_2 in arrayOfArrayProp)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        var destination_3 = new List<D>();
                        foreach (var value_3 in value_2)
                        {
                            if (value_3 == null)
                            {
                                destination_3.Add(null);
                            }
                            else
                            {
                                destination_3.Add(new D(value_3));
                            }
                        }

                        destination_2.Add(destination_3);
                    }
                }

                ArrayOfArrayProp = destination_2;
            }

            if (dictionaryProp != null)
            {
                DictionaryProp = new Dictionary<string, string>(dictionaryProp);
            }

            if (dictionaryWithPrimitiveSchemaProp != null)
            {
                DictionaryWithPrimitiveSchemaProp = new Dictionary<string, double>(dictionaryWithPrimitiveSchemaProp);
            }

            if (dictionaryWithObjectSchemaProp != null)
            {
                DictionaryWithObjectSchemaProp = new Dictionary<string, D>();
                foreach (var value_4 in dictionaryWithObjectSchemaProp)
                {
                    DictionaryWithObjectSchemaProp.Add(value_4.Key, new D(value_4.Value));
                }
            }

            if (dictionaryWithObjectArraySchemaProp != null)
            {
                DictionaryWithObjectArraySchemaProp = new Dictionary<string, IList<D>>();
                foreach (var value_5 in dictionaryWithObjectArraySchemaProp)
                {
                    var destination_4 = new List<D>();
                    foreach (var value_6 in value_5.Value)
                    {
                        if (value_6 == null)
                        {
                            destination_4.Add(null);
                        }
                        else
                        {
                            destination_4.Add(new D(value_6));
                        }
                    }

                    DictionaryWithObjectArraySchemaProp.Add(value_5.Key, destination_4);
                }
            }

            if (dictionaryWithUriKeyProp != null)
            {
                DictionaryWithUriKeyProp = new Dictionary<Uri, D>();
                foreach (var value_7 in dictionaryWithUriKeyProp)
                {
                    DictionaryWithUriKeyProp.Add(value_7.Key, new D(value_7.Value));
                }
            }

            if (dictionaryWithHintedValueProp != null)
            {
                DictionaryWithHintedValueProp = new Dictionary<string, V>(dictionaryWithHintedValueProp);
            }
        }
    }
}";
            const string ExpectedSyntaxInterface =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// An interface for all types generated from the S schema.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public interface ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        SNodeKind SNodeKind { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISNode DeepClone();
    }
}";
            const string ExpectedKindEnum =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// A set of values for all the types that implement <see cref=""ISNode"" />.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum SNodeKind
    {
        /// <summary>
        /// An uninitialized kind.
        /// </summary>
        None,
        /// <summary>
        /// A value indicating that the <see cref=""ISNode"" /> object is of type <see cref=""C"" />.
        /// </summary>
        C,
        /// <summary>
        /// A value indicating that the <see cref=""ISNode"" /> object is of type <see cref=""D"" />.
        /// </summary>
        D
    }
}";
            const string ExpectedRewritingVisitor =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    /// <summary>
    /// Rewriting visitor for the S object model.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public abstract class SRewritingVisitor
    {
        /// <summary>
        /// Starts a rewriting visit of a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object Visit(ISNode node)
        {
            return this.VisitActual(node);
        }

        /// <summary>
        /// Visits and rewrites a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object VisitActual(ISNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(""node"");
            }

            switch (node.SNodeKind)
            {
                case SNodeKind.C:
                    return VisitC((C)node);
                case SNodeKind.D:
                    return VisitD((D)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node) where T : class, ISNode
        {
            if (node == null)
            {
                return null;
            }

            return (T)Visit(node);
        }

        public virtual C VisitC(C node)
        {
            if (node != null)
            {
                node.ReferencedTypeProp = VisitNullChecked(node.ReferencedTypeProp);
                if (node.ArrayOfRefProp != null)
                {
                    for (int index_0 = 0; index_0 < node.ArrayOfRefProp.Count; ++index_0)
                    {
                        node.ArrayOfRefProp[index_0] = VisitNullChecked(node.ArrayOfRefProp[index_0]);
                    }
                }

                if (node.ArrayOfArrayProp != null)
                {
                    for (int index_0 = 0; index_0 < node.ArrayOfArrayProp.Count; ++index_0)
                    {
                        var value_0 = node.ArrayOfArrayProp[index_0];
                        if (value_0 != null)
                        {
                            for (int index_1 = 0; index_1 < value_0.Count; ++index_1)
                            {
                                value_0[index_1] = VisitNullChecked(value_0[index_1]);
                            }
                        }
                    }
                }

                if (node.DictionaryWithObjectSchemaProp != null)
                {
                    var keys = node.DictionaryWithObjectSchemaProp.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.DictionaryWithObjectSchemaProp[key];
                        if (value != null)
                        {
                            node.DictionaryWithObjectSchemaProp[key] = VisitNullChecked(value);
                        }
                    }
                }

                if (node.DictionaryWithObjectArraySchemaProp != null)
                {
                    var keys = node.DictionaryWithObjectArraySchemaProp.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.DictionaryWithObjectArraySchemaProp[key];
                        if (value != null)
                        {
                            for (int index_0 = 0; index_0 < node.DictionaryWithObjectArraySchemaProp[key].Count; ++index_0)
                            {
                                node.DictionaryWithObjectArraySchemaProp[key][index_0] = VisitNullChecked(node.DictionaryWithObjectArraySchemaProp[key][index_0]);
                            }
                        }
                    }
                }

                if (node.DictionaryWithUriKeyProp != null)
                {
                    var keys = node.DictionaryWithUriKeyProp.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.DictionaryWithUriKeyProp[key];
                        if (value != null)
                        {
                            node.DictionaryWithUriKeyProp[key] = VisitNullChecked(value);
                        }
                    }
                }
            }

            return node;
        }

        public virtual D VisitD(D node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}";
            const string ExpectedEnumType =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Some colors.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """+ VersionConstants.FileVersion + @""")]
    public enum Color
    {
        Red,
        Green,
        Blue,
        Black,
        White
    }
}";
            _settings.GenerateCloningCode = true;
            _settings.HintDictionary = new HintDictionary(HintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            generator.Generate(schema);

            string referencedTypePath = TestFileSystem.MakeOutputFilePath("D");
            string enumTypePath = TestFileSystem.MakeOutputFilePath("Color");

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                SyntaxInterfaceOutputFilePath,
                KindEnumOutputFilePath,
                RewritingVisitorOutputFilePath,
                referencedTypePath,
                enumTypePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[enumTypePath].Should().Be(ExpectedEnumType);
            _testFileSystem[PrimaryOutputFilePath].Should().Be(ExpectedClass);
            _testFileSystem[SyntaxInterfaceOutputFilePath].Should().Be(ExpectedSyntaxInterface);
            _testFileSystem[KindEnumOutputFilePath].Should().Be(ExpectedKindEnum);
            _testFileSystem[RewritingVisitorOutputFilePath].Should().Be(ExpectedRewritingVisitor);
        }

        [Fact(DisplayName = "DataModelGenerator generates classes for schemas in definitions")]
        public void GeneratesClassesForSchemasInDefinitions()
        {
            const string ExpectedRootClass =
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

        [DataMember(Name = ""rootProp"", IsRequired = false, EmitDefaultValue = false)]
        public bool RootProp { get; set; }
    }
}";

            const string ExpectedRootEqualityComparerClass =
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

            if (left.RootProp != right.RootProp)
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
                result = (result * 31) + obj.RootProp.GetHashCode();
            }

            return result;
        }
    }
}";

            const string ExpectedRootComparerClass =
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

            compareResult = left.RootProp.CompareTo(right.RootProp);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            const string ExpectedDefinedClass1 =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class Def1
    {
        public static IEqualityComparer<Def1> ValueComparer => Def1EqualityComparer.Instance;

        public bool ValueEquals(Def1 other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<Def1> Comparer => Def1Comparer.Instance;

        [DataMember(Name = ""prop1"", IsRequired = false, EmitDefaultValue = false)]
        public string Prop1 { get; set; }
    }
}";

            const string ExpectedEqualityComparerClass1 =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Def1 for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class Def1EqualityComparer : IEqualityComparer<Def1>
    {
        internal static readonly Def1EqualityComparer Instance = new Def1EqualityComparer();

        public bool Equals(Def1 left, Def1 right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Prop1 != right.Prop1)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Def1 obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Prop1 != null)
                {
                    result = (result * 31) + obj.Prop1.GetHashCode();
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass1 =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Def1 for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class Def1Comparer : IComparer<Def1>
    {
        internal static readonly Def1Comparer Instance = new Def1Comparer();

        public int Compare(Def1 left, Def1 right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Prop1, right.Prop1);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            const string ExpectedDefinedClass2 =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class Def2
    {
        public static IEqualityComparer<Def2> ValueComparer => Def2EqualityComparer.Instance;

        public bool ValueEquals(Def2 other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<Def2> Comparer => Def2Comparer.Instance;

        [DataMember(Name = ""prop2"", IsRequired = false, EmitDefaultValue = false)]
        public int Prop2 { get; set; }
    }
}";

            const string ExpectedEqualityComparerClass2 =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Def2 for equality.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class Def2EqualityComparer : IEqualityComparer<Def2>
    {
        internal static readonly Def2EqualityComparer Instance = new Def2EqualityComparer();

        public bool Equals(Def2 left, Def2 right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Prop2 != right.Prop2)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Def2 obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Prop2.GetHashCode();
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass2 =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Def2 for sorting.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    internal sealed class Def2Comparer : IComparer<Def2>
    {
        internal static readonly Def2Comparer Instance = new Def2Comparer();

        public int Compare(Def2 left, Def2 right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.Prop2.CompareTo(right.Prop2);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Definitions");

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedRootClass,
                    EqualityComparerClassContents = ExpectedRootEqualityComparerClass,
                    ComparerClassContents = ExpectedRootComparerClass
                },
                ["Def1"] = new ExpectedContents
                {
                    ClassContents = ExpectedDefinedClass1,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass1,
                    ComparerClassContents = ExpectedComparerClass1
                },
                ["Def2"] = new ExpectedContents
                {
                    ClassContents = ExpectedDefinedClass2,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass2,
                    ComparerClassContents = ExpectedComparerClass2
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates date-time-valued properties")]
        public void GeneratesDateTimeValuedProperties()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
  ""startTime"": {
    ""type"": ""string"",
    ""format"": ""date-time""
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""startTime"", IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartTime { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (left.StartTime != right.StartTime)
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
                result = (result * 31) + obj.StartTime.GetHashCode();
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.StartTime.CompareTo(right.StartTime);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }


        [Fact(DisplayName = "DataModelGenerator generates URI-valued properties from uri format")]
        public void GeneratesUriValuedPropertiesFromUriFormat()
        {
            string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
  ""targetFile"": {
    ""type"": ""string"",
    ""format"": ""uri""
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""targetFile"", IsRequired = false, EmitDefaultValue = false)]
        public Uri TargetFile { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (left.TargetFile != right.TargetFile)
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
                if (obj.TargetFile != null)
                {
                    result = (result * 31) + obj.TargetFile.GetHashCode();
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.TargetFile.UriCompares(right.TargetFile);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);
            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates URI-valued properties from uri-reference format")]
        public void GeneratesUriValuedPropertiesFromUriReferenceFormat()
        {
            string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
  ""targetFile"": {
    ""type"": ""string"",
    ""format"": ""uri-reference""
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""targetFile"", IsRequired = false, EmitDefaultValue = false)]
        public Uri TargetFile { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (left.TargetFile != right.TargetFile)
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
                if (obj.TargetFile != null)
                {
                    result = (result * 31) + obj.TargetFile.GetHashCode();
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.TargetFile.UriCompares(right.TargetFile);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);
            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates integer property from reference")]
        public void GeneratesIntegerPropertyFromReference()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
    ""intDefProp"": {
      ""$ref"": ""#/definitions/d""
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""integer""
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""intDefProp"", IsRequired = false, EmitDefaultValue = false)]
        public int IntDefProp { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (left.IntDefProp != right.IntDefProp)
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
                result = (result * 31) + obj.IntDefProp.GetHashCode();
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.IntDefProp.CompareTo(right.IntDefProp);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                },
                ["D"] = new ExpectedContents()
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates attributes for properties of primitive type with defaults.")]
        public void GeneratesAttributesForPropertiesOfPrimitiveTypeWithDefaults()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
    ""integerProperty"": {
      ""type"": ""integer"",
      ""description"": ""An integer property.""
    },
    ""integerPropertyWithDefault"": {
      ""type"": ""integer"",
      ""description"": ""An integer property with a default value."",
      ""default"": 42
    },
    ""numberProperty"": {
      ""type"": ""number"",
      ""description"": ""A number property.""
    },
    ""numberPropertyWithDefault"": {
      ""type"": ""number"",
      ""description"": ""A number property with a default value."",
      ""default"": 42.1
    },
    ""stringProperty"": {
      ""type"": ""string"",
      ""description"": ""A string property.""
    },
    ""stringPropertyWithDefault"": {
      ""type"": ""string"",
      ""description"": ""A string property with a default value."",
      ""default"": ""Thanks for all the fish.""
    },
    ""booleanProperty"": {
      ""type"": ""boolean"",
      ""description"": ""A Boolean property.""
    },
    ""booleanPropertyWithTrueDefault"": {
      ""type"": ""boolean"",
      ""description"": ""A Boolean property with a true default value."",
      ""default"": true
    },
    ""booleanPropertyWithFalseDefault"": {
      ""type"": ""boolean"",
      ""description"": ""A Boolean property with a false default value."",
      ""default"": false
    },
    ""nonPrimitivePropertyWithDefault"": {
      ""type"": ""array"",
      ""description"": ""A non-primitive property with a default value: DefaultValue attribute will -not- be emitted."",
      ""items"": {
        ""type"": ""integer""
      },
      ""default"": []
    }
  }
}";
            const string ExpectedClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        /// <summary>
        /// An integer property.
        /// </summary>
        [DataMember(Name = ""integerProperty"", IsRequired = false, EmitDefaultValue = false)]
        public int IntegerProperty { get; set; }

        /// <summary>
        /// An integer property with a default value.
        /// </summary>
        [DataMember(Name = ""integerPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(42)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int IntegerPropertyWithDefault { get; set; }

        /// <summary>
        /// A number property.
        /// </summary>
        [DataMember(Name = ""numberProperty"", IsRequired = false, EmitDefaultValue = false)]
        public double NumberProperty { get; set; }

        /// <summary>
        /// A number property with a default value.
        /// </summary>
        [DataMember(Name = ""numberPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(42.1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double NumberPropertyWithDefault { get; set; }

        /// <summary>
        /// A string property.
        /// </summary>
        [DataMember(Name = ""stringProperty"", IsRequired = false, EmitDefaultValue = false)]
        public string StringProperty { get; set; }

        /// <summary>
        /// A string property with a default value.
        /// </summary>
        [DataMember(Name = ""stringPropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(""Thanks for all the fish."")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StringPropertyWithDefault { get; set; }

        /// <summary>
        /// A Boolean property.
        /// </summary>
        [DataMember(Name = ""booleanProperty"", IsRequired = false, EmitDefaultValue = false)]
        public bool BooleanProperty { get; set; }

        /// <summary>
        /// A Boolean property with a true default value.
        /// </summary>
        [DataMember(Name = ""booleanPropertyWithTrueDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool BooleanPropertyWithTrueDefault { get; set; }

        /// <summary>
        /// A Boolean property with a false default value.
        /// </summary>
        [DataMember(Name = ""booleanPropertyWithFalseDefault"", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool BooleanPropertyWithFalseDefault { get; set; }

        /// <summary>
        /// A non-primitive property with a default value: DefaultValue attribute will -not- be emitted.
        /// </summary>
        [DataMember(Name = ""nonPrimitivePropertyWithDefault"", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<int> NonPrimitivePropertyWithDefault { get; set; }
    }
}";

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of primitive types by $ref")]
        public void GeneratesArrayOfPrimitiveTypeByReference()
        {
            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfIntByRef"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/d""
      }
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""integer"",
      }
    }
  }
}", TestUtil.TestFilePath);

            const string ExpectedClass =
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

        [DataMember(Name = ""arrayOfIntByRef"", IsRequired = false, EmitDefaultValue = false)]
        public IList<int> ArrayOfIntByRef { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (!object.ReferenceEquals(left.ArrayOfIntByRef, right.ArrayOfIntByRef))
            {
                if (left.ArrayOfIntByRef == null || right.ArrayOfIntByRef == null)
                {
                    return false;
                }

                if (left.ArrayOfIntByRef.Count != right.ArrayOfIntByRef.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfIntByRef.Count; ++index_0)
                {
                    if (left.ArrayOfIntByRef[index_0] != right.ArrayOfIntByRef[index_0])
                    {
                        return false;
                    }
                }
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
                if (obj.ArrayOfIntByRef != null)
                {
                    foreach (var value_0 in obj.ArrayOfIntByRef)
                    {
                        result = result * 31;
                        result = (result * 31) + value_0.GetHashCode();
                    }
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.ArrayOfIntByRef.ListCompares(right.ArrayOfIntByRef);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfPrimitiveTypeByReference));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass,
                },
                ["D"] = new ExpectedContents()
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of uri-formatted strings")]
        public void GeneratesArrayOfUriFormattedStrings()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""uriFormattedStrings"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""string"",
        ""format"": ""uri""
      }
    }
  }
}", TestUtil.TestFilePath);

            const string ExpectedClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : ISNode
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<C> Comparer => CComparer.Instance;

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        public SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

        [DataMember(Name = ""uriFormattedStrings"", IsRequired = false, EmitDefaultValue = false)]
        public IList<Uri> UriFormattedStrings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class.
        /// </summary>
        public C()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the supplied values.
        /// </summary>
        /// <param name=""uriFormattedStrings"">
        /// An initialization value for the <see cref=""P:UriFormattedStrings"" /> property.
        /// </param>
        public C(IEnumerable<Uri> uriFormattedStrings)
        {
            Init(uriFormattedStrings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the specified instance.
        /// </summary>
        /// <param name=""other"">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref=""ArgumentNullException"">
        /// Thrown if <paramref name=""other"" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.UriFormattedStrings);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        private void Init(IEnumerable<Uri> uriFormattedStrings)
        {
            if (uriFormattedStrings != null)
            {
                var destination_0 = new List<Uri>();
                foreach (var value_0 in uriFormattedStrings)
                {
                    destination_0.Add(value_0);
                }

                UriFormattedStrings = destination_0;
            }
        }
    }
}";

            const string ExpectedEqualityComparerClass =
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

            if (!object.ReferenceEquals(left.UriFormattedStrings, right.UriFormattedStrings))
            {
                if (left.UriFormattedStrings == null || right.UriFormattedStrings == null)
                {
                    return false;
                }

                if (left.UriFormattedStrings.Count != right.UriFormattedStrings.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.UriFormattedStrings.Count; ++index_0)
                {
                    if (left.UriFormattedStrings[index_0] != right.UriFormattedStrings[index_0])
                    {
                        return false;
                    }
                }
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
                if (obj.UriFormattedStrings != null)
                {
                    foreach (var value_0 in obj.UriFormattedStrings)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.UriFormattedStrings.ListCompares(right.UriFormattedStrings);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            const string ExpectedSyntaxInterface =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// An interface for all types generated from the S schema.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public interface ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        SNodeKind SNodeKind { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISNode DeepClone();
    }
}";

            const string ExpectedKindEnum =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// A set of values for all the types that implement <see cref=""ISNode"" />.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum SNodeKind
    {
        /// <summary>
        /// An uninitialized kind.
        /// </summary>
        None,
        /// <summary>
        /// A value indicating that the <see cref=""ISNode"" /> object is of type <see cref=""C"" />.
        /// </summary>
        C
    }
}";

            const string ExpectedRewritingVisitor =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    /// <summary>
    /// Rewriting visitor for the S object model.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public abstract class SRewritingVisitor
    {
        /// <summary>
        /// Starts a rewriting visit of a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object Visit(ISNode node)
        {
            return this.VisitActual(node);
        }

        /// <summary>
        /// Visits and rewrites a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object VisitActual(ISNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(""node"");
            }

            switch (node.SNodeKind)
            {
                case SNodeKind.C:
                    return VisitC((C)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node) where T : class, ISNode
        {
            if (node == null)
            {
                return null;
            }

            return (T)Visit(node);
        }

        public virtual C VisitC(C node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            _settings.GenerateCloningCode = true;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            generator.Generate(schema);

            string equalityComparerOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName + "EqualityComparer");
            string comparerOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName + "Comparer");
            string comparerExtensionFilePath = TestFileSystem.MakeOutputFilePath("ComparerExtensions");
            string syntaxInterfacePath = TestFileSystem.MakeOutputFilePath("ISNode");
            string kindEnumPath = TestFileSystem.MakeOutputFilePath("SNodeKind");
            string rewritingVisitorClassPath = TestFileSystem.MakeOutputFilePath("SRewritingVisitor");

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                equalityComparerOutputFilePath,
                comparerOutputFilePath,
                comparerExtensionFilePath,
                syntaxInterfacePath,
                kindEnumPath,
                rewritingVisitorClassPath,
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(ExpectedClass);
            _testFileSystem[equalityComparerOutputFilePath].Should().Be(ExpectedEqualityComparerClass);
            _testFileSystem[comparerOutputFilePath].Should().Be(ExpectedComparerClass);
            _testFileSystem[syntaxInterfacePath].Should().Be(ExpectedSyntaxInterface);
            _testFileSystem[kindEnumPath].Should().Be(ExpectedKindEnum);
            _testFileSystem[rewritingVisitorClassPath].Should().Be(ExpectedRewritingVisitor);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of primitive type")]
        public void GeneratesArrayOfArraysOfPrimitiveType()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfArrayOfInt"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/itemType""
      }
    }
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""integer""
      }
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""arrayOfArrayOfInt"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<int>> ArrayOfArrayOfInt { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (!object.ReferenceEquals(left.ArrayOfArrayOfInt, right.ArrayOfArrayOfInt))
            {
                if (left.ArrayOfArrayOfInt == null || right.ArrayOfArrayOfInt == null)
                {
                    return false;
                }

                if (left.ArrayOfArrayOfInt.Count != right.ArrayOfArrayOfInt.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfArrayOfInt.Count; ++index_0)
                {
                    if (!object.ReferenceEquals(left.ArrayOfArrayOfInt[index_0], right.ArrayOfArrayOfInt[index_0]))
                    {
                        if (left.ArrayOfArrayOfInt[index_0] == null || right.ArrayOfArrayOfInt[index_0] == null)
                        {
                            return false;
                        }

                        if (left.ArrayOfArrayOfInt[index_0].Count != right.ArrayOfArrayOfInt[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < left.ArrayOfArrayOfInt[index_0].Count; ++index_1)
                        {
                            if (left.ArrayOfArrayOfInt[index_0][index_1] != right.ArrayOfArrayOfInt[index_0][index_1])
                            {
                                return false;
                            }
                        }
                    }
                }
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
                if (obj.ArrayOfArrayOfInt != null)
                {
                    foreach (var value_0 in obj.ArrayOfArrayOfInt)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                result = (result * 31) + value_1.GetHashCode();
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.ArrayOfArrayOfInt.ListCompares(right.ArrayOfArrayOfInt);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfArraysOfPrimitiveType));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass,
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of object type")]
        public void GeneratesArrayOfArraysOfObjectType()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfArrayOfObject"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/itemType""
      }
    }
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object""
      }
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""arrayOfArrayOfObject"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<object>> ArrayOfArrayOfObject { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (!object.ReferenceEquals(left.ArrayOfArrayOfObject, right.ArrayOfArrayOfObject))
            {
                if (left.ArrayOfArrayOfObject == null || right.ArrayOfArrayOfObject == null)
                {
                    return false;
                }

                if (left.ArrayOfArrayOfObject.Count != right.ArrayOfArrayOfObject.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfArrayOfObject.Count; ++index_0)
                {
                    if (!object.ReferenceEquals(left.ArrayOfArrayOfObject[index_0], right.ArrayOfArrayOfObject[index_0]))
                    {
                        if (left.ArrayOfArrayOfObject[index_0] == null || right.ArrayOfArrayOfObject[index_0] == null)
                        {
                            return false;
                        }

                        if (left.ArrayOfArrayOfObject[index_0].Count != right.ArrayOfArrayOfObject[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < left.ArrayOfArrayOfObject[index_0].Count; ++index_1)
                        {
                            if (!object.Equals(left.ArrayOfArrayOfObject[index_0][index_1], right.ArrayOfArrayOfObject[index_0][index_1]))
                            {
                                return false;
                            }
                        }
                    }
                }
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
                if (obj.ArrayOfArrayOfObject != null)
                {
                    foreach (var value_0 in obj.ArrayOfArrayOfObject)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                if (value_1 != null)
                                {
                                    result = (result * 31) + value_1.GetHashCode();
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.ArrayOfArrayOfObject.ListCompares(right.ArrayOfArrayOfObject);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfArraysOfObjectType));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of class type")]
        public void GeneratesArrayOfArraysOfClassType()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfArrayOfD"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/itemType""
      }
    }
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/d""
      }
    },
    ""d"": {
      ""type"": ""object"",
      }
    }
  }
}";

            const string ExpectedClass =
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

        [DataMember(Name = ""arrayOfArrayOfD"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<D>> ArrayOfArrayOfD { get; set; }
    }
}";
            const string ExpectedEqualityComparerClass =
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

            if (!object.ReferenceEquals(left.ArrayOfArrayOfD, right.ArrayOfArrayOfD))
            {
                if (left.ArrayOfArrayOfD == null || right.ArrayOfArrayOfD == null)
                {
                    return false;
                }

                if (left.ArrayOfArrayOfD.Count != right.ArrayOfArrayOfD.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfArrayOfD.Count; ++index_0)
                {
                    if (!object.ReferenceEquals(left.ArrayOfArrayOfD[index_0], right.ArrayOfArrayOfD[index_0]))
                    {
                        if (left.ArrayOfArrayOfD[index_0] == null || right.ArrayOfArrayOfD[index_0] == null)
                        {
                            return false;
                        }

                        if (left.ArrayOfArrayOfD[index_0].Count != right.ArrayOfArrayOfD[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < left.ArrayOfArrayOfD[index_0].Count; ++index_1)
                        {
                            if (!D.ValueComparer.Equals(left.ArrayOfArrayOfD[index_0][index_1], right.ArrayOfArrayOfD[index_0][index_1]))
                            {
                                return false;
                            }
                        }
                    }
                }
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
                if (obj.ArrayOfArrayOfD != null)
                {
                    foreach (var value_0 in obj.ArrayOfArrayOfD)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                if (value_1 != null)
                                {
                                    result = (result * 31) + value_1.ValueGetHashCode();
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = left.ArrayOfArrayOfD.ListCompares(right.ArrayOfArrayOfD);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            _settings.GenerateEqualityComparers = true;
            _settings.GenerateComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfArraysOfClassType));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    EqualityComparerClassContents = ExpectedEqualityComparerClass,
                    ComparerClassContents = ExpectedComparerClass
                },
                ["D"] = new ExpectedContents()
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates string for inline enum of string")]
        public void GeneratesStringForInlineEnumOfString()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
    @"{
  ""type"": ""object"",
  ""properties"": {
    ""version"": {
      ""enum"": [
        ""v1.0"",
        ""v2.0""
      ]
    }
  }
}", TestUtil.TestFilePath);

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""version"", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates attributes for required and optional properties")]
        public void GeneratesAttributesForRequiredAndOptionalProperties()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""requiredProp1"": {
      ""type"": ""string""
    },
    ""optionalProp"": {
      ""type"": ""string""
    },
    ""requiredProp2"": {
      ""type"": ""string""
    }
  },
  ""required"": [ ""requiredProp1"", ""requiredProp2"" ]
}", TestUtil.TestFilePath);

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C
    {
        [DataMember(Name = ""requiredProp1"", IsRequired = true)]
        public string RequiredProp1 { get; set; }
        [DataMember(Name = ""optionalProp"", IsRequired = false, EmitDefaultValue = false)]
        public string OptionalProp { get; set; }
        [DataMember(Name = ""requiredProp2"", IsRequired = true)]
        public string RequiredProp2 { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates sealed classes when option is set")]
        public void GeneratesSealedClassesWhenOptionIsSet()
        {
            _settings.SealClasses = true;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
}", TestUtil.TestFilePath);

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public sealed class C
    {
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates protected Init methods when option is set")]
        public void GeneratesProtectedInitMethodsWhenOptionIsSet()
        {
            _settings.ProtectedInitMethods = true;

            // Unless you generate cloning code, you don't get an Init method at all.
            _settings.GenerateCloningCode = true;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""prop"": {
      ""type"": ""string""
    }
  }
}", TestUtil.TestFilePath);

            const string ExpectedClass =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        public SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

        [DataMember(Name = ""prop"", IsRequired = false, EmitDefaultValue = false)]
        public string Prop { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class.
        /// </summary>
        public C()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the supplied values.
        /// </summary>
        /// <param name=""prop"">
        /// An initialization value for the <see cref=""P:Prop"" /> property.
        /// </param>
        public C(string prop)
        {
            Init(prop);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the specified instance.
        /// </summary>
        /// <param name=""other"">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref=""ArgumentNullException"">
        /// Thrown if <paramref name=""other"" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Prop);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        protected virtual void Init(string prop)
        {
            Prop = prop;
        }
    }
}";

            string actualClass = generator.Generate(schema);
            actualClass.Should().Be(ExpectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates virtual members when option is set")]
        public void GeneratesVirtualMembersWhenOptionIsSet()
        {
            _settings.VirtualMembers = true;
            _settings.GenerateComparers = true;
            _settings.GenerateCloningCode = true;
            _settings.GenerateEqualityComparers = true;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""prop"": {
      ""type"": ""string""
    }
  }
}", TestUtil.TestFilePath);

            const string ExpectedClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public partial class C : ISNode
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        public static IComparer<C> Comparer => CComparer.Instance;

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        public virtual SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

        [DataMember(Name = ""prop"", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Prop { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class.
        /// </summary>
        public C()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the supplied values.
        /// </summary>
        /// <param name=""prop"">
        /// An initialization value for the <see cref=""P:Prop"" /> property.
        /// </param>
        public C(string prop)
        {
            Init(prop);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the specified instance.
        /// </summary>
        /// <param name=""other"">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref=""ArgumentNullException"">
        /// Thrown if <paramref name=""other"" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Prop);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        private void Init(string prop)
        {
            Prop = prop;
        }
    }
}";

            const string ExpectedEqualityComparerClass =
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

            if (left.Prop != right.Prop)
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
                if (obj.Prop != null)
                {
                    result = (result * 31) + obj.Prop.GetHashCode();
                }
            }

            return result;
        }
    }
}";

            const string ExpectedComparerClass =
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

            compareResult = string.Compare(left.Prop, right.Prop);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}";

            const string ExpectedSyntaxInterface =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// An interface for all types generated from the S schema.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public interface ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        SNodeKind SNodeKind { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISNode DeepClone();
    }
}";
            const string ExpectedKindEnum =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// A set of values for all the types that implement <see cref=""ISNode"" />.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public enum SNodeKind
    {
        /// <summary>
        /// An uninitialized kind.
        /// </summary>
        None,
        /// <summary>
        /// A value indicating that the <see cref=""ISNode"" /> object is of type <see cref=""C"" />.
        /// </summary>
        C
    }
}";
            const string ExpectedRewritingVisitor =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    /// <summary>
    /// Rewriting visitor for the S object model.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", """ + VersionConstants.FileVersion + @""")]
    public abstract class SRewritingVisitor
    {
        /// <summary>
        /// Starts a rewriting visit of a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object Visit(ISNode node)
        {
            return this.VisitActual(node);
        }

        /// <summary>
        /// Visits and rewrites a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object VisitActual(ISNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(""node"");
            }

            switch (node.SNodeKind)
            {
                case SNodeKind.C:
                    return VisitC((C)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node) where T : class, ISNode
        {
            if (node == null)
            {
                return null;
            }

            return (T)Visit(node);
        }

        public virtual C VisitC(C node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}";

            generator.Generate(schema);

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                PrimaryComparerOutputFilePath,
                PrimaryEqualityComparerOutputFilePath,
                ComparerExtensionsOutputFilePath,
                SyntaxInterfaceOutputFilePath,
                KindEnumOutputFilePath,
                RewritingVisitorOutputFilePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(ExpectedClass);
            _testFileSystem[PrimaryComparerOutputFilePath].Should().Be(ExpectedComparerClass);
            _testFileSystem[PrimaryEqualityComparerOutputFilePath].Should().Be(ExpectedEqualityComparerClass);
            _testFileSystem[SyntaxInterfaceOutputFilePath].Should().Be(ExpectedSyntaxInterface);
            _testFileSystem[KindEnumOutputFilePath].Should().Be(ExpectedKindEnum);
            _testFileSystem[RewritingVisitorOutputFilePath].Should().Be(ExpectedRewritingVisitor);
        }

        [Fact(DisplayName = "DataModelGenerator accepts a limited oneOf with \"type\": \"null\"")]
        public void AcceptsLimitedOneOfWithTypeNull()
        {
            const string Schema =
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOrNullProperty"": {
      ""description"": ""A property that can either be an array or null."",
      ""oneOf"": [
        {
          ""type"": ""array""
        },
        {
          ""type"": ""null""
        }
      ],
      ""items"": {
        ""type"": ""integer""
      }
    }
  }
}";
            const string ExpectedClass =
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
        /// <summary>
        /// A property that can either be an array or null.
        /// </summary>
        [DataMember(Name = ""arrayOrNullProperty"", IsRequired = false, EmitDefaultValue = false)]
        public IList<int> ArrayOrNullProperty { get; set; }
    }
}";

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        private void VerifyGeneratedFileContents(IDictionary<string, ExpectedContents> expectedContentsDictionary)
        {
            Assert.FileContentsMatchExpectedContents(_testFileSystem, expectedContentsDictionary, _settings.GenerateEqualityComparers, _settings.GenerateComparers);
        }
    }
}
