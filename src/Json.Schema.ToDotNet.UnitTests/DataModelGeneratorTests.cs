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
        private const string ClassName = nameof(DataModelGeneratorTests);
        private static readonly string PrimaryOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName);

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
        public void DoesNotThrowIfForceOverwriteSettingIsSet()
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
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            string actual = generator.Generate(schema);
            actual.Should().Be(expectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates properties with built-in types")]
        public void GeneratesPropertiesWithBuiltInTypes()
        {
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass.cs");


            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Properties");

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass,
                    ComparerClassContents = expectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates object-valued property with correct type")]
        public void GeneratesObjectValuedPropertyWithCorrectType()
        {
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass.cs");

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Object");

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass,
                    ComparerClassContents = expectedComparerClass
                },

                ["D"] = new ExpectedContents()
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference is not a fragment")]
        public void ThrowsIfReferenceIsNotAFragment()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*https://example.com/pschema.schema.json/#*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference does not specify a definition")]
        public void ThrowsIfReferenceDoesNotSpecifyADefinition()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*#/notDefinitions/p*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if referenced definition does not exist")]
        public void ThrowsIfReferencedDefinitionDoesNotExist()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*nonExistentDefinition*");
        }

        [Fact(DisplayName = "DataModelGenerator generates array-valued property")]
        public void GeneratesArrayValuedProperty()
        {
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass.cs");

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Array");

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(expectedClass, actual, nameof(GeneratesArrayValuedProperty));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass,
                    ComparerClassContents = expectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties")]
        public void GeneratesXmlCommentsForProperties()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");

            string actual = generator.Generate(schema);
            actual.Should().Be(expectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties whose property type is ref")]
        public void GeneratesXmlCommentsForPropertiesWhosePropertyTypeIsRef()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");
            string expectedRootClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedRootClass.cs");
            string expectedRootComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedRootComparerClass.cs");
            string expectedColorClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedColorClass.cs");
            string expectedColorComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedColorComparerClass.cs");

            _settings.GenerateEqualityComparers = true;
            _settings.RootClassName = "ConsoleWindow";
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedRootClass,
                    ComparerClassContents = expectedRootComparerClass
                },
                ["Color"] = new ExpectedContents
                {
                    ClassContents = expectedColorClass,
                    ComparerClassContents = expectedColorComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates copyright notice")]
        public void GeneratesCopyrightAtTopOfFile()
        {
            _settings.CopyrightNotice = TestUtil.ReadTestInputFile(ClassName, "CopyrightNotice.txt");
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");

            string actual = generator.Generate(schema);
            actual.Should().Be(expectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates cloning code")]
        public void GeneratesCloningCode()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string hintsText = TestUtil.ReadTestInputFile(ClassName, "CodeGenHints.json");
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedSyntaxInterface = TestUtil.ReadTestInputFile(ClassName, "ExpectedSyntaxInterface.cs");
            string expectedKindEnum = TestUtil.ReadTestInputFile(ClassName, "ExpectedKindEnum.cs");
            string expectedRewritingVisitor = TestUtil.ReadTestInputFile(ClassName, "ExpectedRewritingVisitor.cs");
            string expectedEnumType = TestUtil.ReadTestInputFile(ClassName, "ExpectedEnumType.cs");

            _settings.GenerateCloningCode = true;
            _settings.HintDictionary = new HintDictionary(hintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            generator.Generate(schema);

            string syntaxInterfacePath = TestFileSystem.MakeOutputFilePath("ISNode");
            string kindEnumPath = TestFileSystem.MakeOutputFilePath("SNodeKind");
            string referencedTypePath = TestFileSystem.MakeOutputFilePath("D");
            string rewritingVisitorClassPath = TestFileSystem.MakeOutputFilePath("SRewritingVisitor");
            string enumTypePath = TestFileSystem.MakeOutputFilePath("Color");

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                syntaxInterfacePath,
                kindEnumPath,
                rewritingVisitorClassPath,
                referencedTypePath,
                enumTypePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[enumTypePath].Should().Be(expectedEnumType);
            _testFileSystem[PrimaryOutputFilePath].Should().Be(expectedClass);
            _testFileSystem[syntaxInterfacePath].Should().Be(expectedSyntaxInterface);
            _testFileSystem[kindEnumPath].Should().Be(expectedKindEnum);
            _testFileSystem[rewritingVisitorClassPath].Should().Be(expectedRewritingVisitor);
        }

        [Fact(DisplayName = "DataModelGenerator generates classes for schemas in definitions")]
        public void GeneratesClassesForSchemasInDefinitions()
        {
            string expectedRootClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedRootClass.cs");
            string expectedRootComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedRootComparerClass.cs");
            string expectedDefinedClass1 = TestUtil.ReadTestInputFile(ClassName, "ExpectedDefinedClass1.cs");
            string expectedComparerClass1 = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass1.cs");
            string expectedDefinedClass2 = TestUtil.ReadTestInputFile(ClassName, "ExpectedDefinedClass2.cs");
            string expectedComparerClass2 = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass2.cs");

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Definitions");

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedRootClass,
                    ComparerClassContents = expectedRootComparerClass
                },
                ["Def1"] = new ExpectedContents
                {
                    ClassContents = expectedDefinedClass1,
                    ComparerClassContents = expectedComparerClass1
                },
                ["Def2"] = new ExpectedContents
                {
                    ClassContents = expectedDefinedClass2,
                    ComparerClassContents = expectedComparerClass2
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates date-time-valued properties")]
        public void GeneratesDateTimeValuedProperties()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass.cs");

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass,
                    ComparerClassContents = expectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }


        [Fact(DisplayName = "DataModelGenerator generates URI-valued properties from uri format")]
        public void GeneratesUriValuedPropertiesFromUriFormat()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass.cs");

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            generator.Generate(schema);
            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass,
                    ComparerClassContents = expectedComparerClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates URI-valued properties from uri-reference format")]
        public void GeneratesUriValuedPropertiesFromUriReferenceFormat()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, "Schema.json");
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedClass.cs");
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, "ExpectedComparerClass.cs");

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            generator.Generate(schema);
            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass,
                    ComparerClassContents = expectedComparerClass
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

        [DataMember(Name = ""intDefProp"", IsRequired = false, EmitDefaultValue = false)]
        public int IntDefProp { get; set; }
    }
}";
            const string ExpectedComparerClass =
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

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
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

        [DataMember(Name = ""arrayOfIntByRef"", IsRequired = false, EmitDefaultValue = false)]
        public IList<int> ArrayOfIntByRef { get; set; }
    }
}";
            const string ExpectedComparerClass =
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

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfPrimitiveTypeByReference));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    ComparerClassContents = ExpectedComparerClass
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

            const string ExpectedComparerClass =
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
            _settings.GenerateCloningCode = true;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            generator.Generate(schema);

            string equalityComparerOutputFilePath = TestFileSystem.MakeOutputFilePath(TestSettings.RootClassName + "EqualityComparer");
            string syntaxInterfacePath = TestFileSystem.MakeOutputFilePath("ISNode");
            string kindEnumPath = TestFileSystem.MakeOutputFilePath("SNodeKind");
            string rewritingVisitorClassPath = TestFileSystem.MakeOutputFilePath("SRewritingVisitor");

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                equalityComparerOutputFilePath,
                syntaxInterfacePath,
                kindEnumPath,
                rewritingVisitorClassPath,
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(ExpectedClass);
            _testFileSystem[equalityComparerOutputFilePath].Should().Be(ExpectedComparerClass);
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

        [DataMember(Name = ""arrayOfArrayOfInt"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<int>> ArrayOfArrayOfInt { get; set; }
    }
}";
            const string ExpectedComparerClass =
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
            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfArraysOfPrimitiveType));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
                    ComparerClassContents = ExpectedComparerClass
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

        [DataMember(Name = ""arrayOfArrayOfObject"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<object>> ArrayOfArrayOfObject { get; set; }
    }
}";
            const string ExpectedComparerClass =
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

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfArraysOfObjectType));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
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

        [DataMember(Name = ""arrayOfArrayOfD"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<D>> ArrayOfArrayOfD { get; set; }
    }
}";
            const string ExpectedComparerClass =
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

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(Schema, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(ExpectedClass, actual, nameof(GeneratesArrayOfArraysOfClassType));

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = ExpectedClass,
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
            Assert.FileContentsMatchExpectedContents(_testFileSystem, expectedContentsDictionary, _settings.GenerateEqualityComparers);
        }
    }
}
