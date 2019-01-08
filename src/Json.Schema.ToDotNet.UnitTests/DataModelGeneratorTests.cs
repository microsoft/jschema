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

        // File names for commonly used input data files.
        private const string SchemaFileName = "Schema.json";
        private const string HintsFileName = "CodeGenHints.json";

        // File names for commonly used expected result files.
        private const string ExpectedClassFileName = "ExpectedClass.cs";
        private const string ExpectedComparerClassFileName = "ExpectedComparerClass.cs";
        private const string ExpectedSyntaxInterfaceFileName = "ExpectedSyntaxInterface.cs";
        private const string ExpectedKindEnumFileName = "ExpectedKindEnum.cs";
        private const string ExpectedEnumTypeFileName = "ExpectedEnumType.cs";
        private const string ExpectedRewritingVisitorFileName = "ExpectedRewritingVisitor.cs";

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
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            string actual = generator.Generate(schema);
            actual.Should().Be(expectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates properties with built-in types")]
        public void GeneratesPropertiesWithBuiltInTypes()
        {
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);


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
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

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
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*https://example.com/pschema.schema.json/#*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference does not specify a definition")]
        public void ThrowsIfReferenceDoesNotSpecifyADefinition()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*#/notDefinitions/p*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if referenced definition does not exist")]
        public void ThrowsIfReferencedDefinitionDoesNotExist()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);

            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.Should().Throw<ApplicationException>()
                .WithMessage("*nonExistentDefinition*");
        }

        [Fact(DisplayName = "DataModelGenerator generates array-valued property")]
        public void GeneratesArrayValuedProperty()
        {
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

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

            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);

            string actual = generator.Generate(schema);
            actual.Should().Be(expectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties whose property type is ref")]
        public void GeneratesXmlCommentsForPropertiesWhosePropertyTypeIsRef()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
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

            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);

            string actual = generator.Generate(schema);
            actual.Should().Be(expectedClass);
        }

        [Fact(DisplayName = "DataModelGenerator generates cloning code")]
        public void GeneratesCloningCode()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string hintsText = TestUtil.ReadTestInputFile(ClassName, HintsFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedSyntaxInterface = TestUtil.ReadTestInputFile(ClassName, ExpectedSyntaxInterfaceFileName);
            string expectedKindEnum = TestUtil.ReadTestInputFile(ClassName, ExpectedKindEnumFileName);
            string expectedRewritingVisitor = TestUtil.ReadTestInputFile(ClassName, ExpectedRewritingVisitorFileName);
            string expectedEnumType = TestUtil.ReadTestInputFile(ClassName, ExpectedEnumTypeFileName);

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
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

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
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

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
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

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
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

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
                },
                ["D"] = new ExpectedContents()
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates attributes for properties of primitive type with defaults.")]
        public void GeneratesAttributesForPropertiesOfPrimitiveTypeWithDefaults()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            generator.Generate(schema);

            var expectedContentsDictionary = new Dictionary<string, ExpectedContents>
            {
                [_settings.RootClassName] = new ExpectedContents
                {
                    ClassContents = expectedClass
                }
            };

            VerifyGeneratedFileContents(expectedContentsDictionary);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of primitive types by $ref")]
        public void GeneratesArrayOfPrimitiveTypeByReference()
        {
            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(expectedClass, actual, nameof(GeneratesArrayOfPrimitiveTypeByReference));

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

        [Fact(DisplayName = "DataModelGenerator generates array of uri-formatted strings")]
        public void GeneratesArrayOfUriFormattedStrings()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);
            string expectedSyntaxInterface = TestUtil.ReadTestInputFile(ClassName, ExpectedSyntaxInterfaceFileName);
            string expectedKindEnum = TestUtil.ReadTestInputFile(ClassName, ExpectedKindEnumFileName);
            string expectedRewritingVisitor = TestUtil.ReadTestInputFile(ClassName, ExpectedRewritingVisitorFileName);

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

            _testFileSystem[PrimaryOutputFilePath].Should().Be(expectedClass);
            _testFileSystem[equalityComparerOutputFilePath].Should().Be(expectedComparerClass);
            _testFileSystem[syntaxInterfacePath].Should().Be(expectedSyntaxInterface);
            _testFileSystem[kindEnumPath].Should().Be(expectedKindEnum);
            _testFileSystem[rewritingVisitorClassPath].Should().Be(expectedRewritingVisitor);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of primitive type")]
        public void GeneratesArrayOfArraysOfPrimitiveType()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(expectedClass, actual, nameof(GeneratesArrayOfArraysOfPrimitiveType));

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

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of object type")]
        public void GeneratesArrayOfArraysOfObjectType()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(expectedClass, actual, nameof(GeneratesArrayOfArraysOfObjectType));

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

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of class type")]
        public void GeneratesArrayOfArraysOfClassType()
        {
            string schemaText = TestUtil.ReadTestInputFile(ClassName, SchemaFileName);
            string expectedClass = TestUtil.ReadTestInputFile(ClassName, ExpectedClassFileName);
            string expectedComparerClass = TestUtil.ReadTestInputFile(ClassName, ExpectedComparerClassFileName);

            _settings.GenerateEqualityComparers = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, TestUtil.TestFilePath);

            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(expectedClass, actual, nameof(GeneratesArrayOfArraysOfClassType));

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
