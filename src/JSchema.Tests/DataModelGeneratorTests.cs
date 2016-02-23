// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Microsoft.JSchema.Tests;
using Xunit;

namespace Microsoft.JSchema.Generator.Tests
{
    public class DataModelGeneratorTests
    {
        private const string CopyrightFilePath = @"C:\copyright.txt";

        private Mock<IFileSystem> _mockFileSystem;
        private IFileSystem _fileSystem;
        private readonly DataModelGeneratorSettings _settings;
        private readonly Dictionary<string, string> _fileContentsDictionary;

        public DataModelGeneratorTests()
        {
            _fileSystem = MakeFileSystem();
            _settings = MakeSettings();
            _fileContentsDictionary = new Dictionary<string, string>();
        }

        [Fact(DisplayName = "DataModelGenerator throws if output directory exists")]
        public void ThrowsIfOutputDirectoryExists()
        {
            _settings.ForceOverwrite = false;

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            // ... and the message should mention the output directory.
            action.ShouldThrow<JSchemaException>().WithMessage($"*{OutputDirectory}*");
        }

        [Fact(DisplayName = "DataModelGenerator does not throw if output directory does not exist")]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            // Use a directory name other than the default. The mock file system believes
            // that only the default directory exists.
            _settings.OutputDirectory = _settings.OutputDirectory + "x";

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldNotThrow();
        }

        [Fact(DisplayName = "DataModelGenerator does not throw if ForceOverwrite setting is set")]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            // This is the default from MakeSettings; restated here for explicitness.
            _settings.ForceOverwrite = true;

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldNotThrow();
        }

        [Fact(DisplayName = "DataModelGenerator throws if copyright file does not exist")]
        public void ThrowsIfCopyrightFileDoesNotExist()
        {
            _settings.CopyrightFilePath = CopyrightFilePath;

            _mockFileSystem.Setup(fs => fs.FileExists(CopyrightFilePath))
                .Returns(false);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            // ... and the exception message should mention the file path.
            action.ShouldThrow<JSchemaException>().WithMessage($"*{CopyrightFilePath}*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if root schema is not of type 'object'")]
        public void ThrowsIfRootSchemaIsNotOfTypeObject()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("NotAnObject");

            Action action = () => generator.Generate(schema);

            // ... and the message should mention what the root type actually was.
            action.ShouldThrow<JSchemaException>().WithMessage("*number*");
        }

        [Fact(DisplayName = "DataModelGenerator generates properties with built-in types")]
        public void GeneratesPropertiesWithBuiltInTypes()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Properties");

            string actual = generator.Generate(schema);

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public string StringProp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double NumberProp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool BooleanProp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int IntegerProp { get; set; }
    }
}";
            actual.Should().Be(Expected);

            // This particular test shows not only that the correct text was produced,
            // but that it was written to the expected path. Subsequent tests will
            // just verify the text.
            _fileContentsDictionary.Keys.Should().OnlyContain(key => key.Equals(@"Generated\C.cs"));
        }

        [Fact(DisplayName = "DataModelGenerator generates object-valued property with correct type")]
        public void GeneratesObjectValuedPropertyWithCorrectType()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Object");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public D ObjectProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
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
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText);
            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldThrow<JSchemaException>()
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
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText);
            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldThrow<JSchemaException>()
                .WithMessage("*#/notDefinitions/p*");
        }

        [Fact(DisplayName = "Throws if referenced definition does not exist")]
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
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText);
            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldThrow<JSchemaException>()
                .WithMessage("*nonExistentDefinition*");
        }

        [Fact(DisplayName = "DataModelGenerator generates array-valued property")]
        public void GeneratesArrayValuedProperty()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Array");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public object[] ArrayProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties")]
        public void GeneratesXmlCommentsForProperties()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// An example property.
        /// </summary>
        public string ExampleProp { get; set; }
    }
}";

            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates copyright notice")]
        public void GeneratesCopyrightAtTopOfFile()
        {
            const string CopyrightNotice =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.
";
            _settings.CopyrightFilePath = CopyrightFilePath;

            _mockFileSystem.Setup(fs => fs.FileExists(CopyrightFilePath))
                .Returns(true);
            _mockFileSystem.Setup(fs => fs.ReadAllText(CopyrightFilePath))
                .Returns(CopyrightNotice);
            _fileSystem = _mockFileSystem.Object;

            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.

namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// An example property.
        /// </summary>
        public string ExampleProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates property for enum with Boolean values")]
        public void GeneratesPropertyForEnumWithBooleanValues()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("BooleanEnum");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public bool BooleanEnumProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates property for enum with integer values")]
        public void GeneratesPropertyForEnumWithIntegerValues()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("IntegerEnum");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public int IntegerEnumProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates property for enum with mixed values")]
        public void GeneratesPropertyForEnumWithMixedValues()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("MixedEnum");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public object MixedEnumProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates property for enum with number values")]
        public void GeneratesPropertyForEnumWithNumberValues()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("NumberEnum");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public double NumberEnumProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates property for enum with string values")]
        public void GeneratesPropertyForEnumWithStringValues()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("StringEnum");

            const string Expected =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public string StringEnumProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates classes for schemas in definitions")]
        public void GeneratesClassesForSchemasInDefinitions()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Definitions");

            generator.Generate(schema);

            const string ExpectedRootClass =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public bool RootProp { get; set; }
    }
}";

            const string ExpectedDefinedClass1 =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Def1
    {
        /// <summary>
        /// 
        /// </summary>
        public string Prop1 { get; set; }
    }
}";

            const string ExpectedDefinedClass2 =
@"namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Def2
    {
        /// <summary>
        /// 
        /// </summary>
        public int Prop2 { get; set; }
    }
}";
            var expectedOutputFiles = new List<string>
            {
                @"Generated\C.cs",
                @"Generated\Def1.cs",
                @"Generated\Def2.cs"
            };

            _fileContentsDictionary.Count.Should().Be(expectedOutputFiles.Count);
            _fileContentsDictionary.Keys.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

            _fileContentsDictionary[@"Generated\C.cs"].Should().Be(ExpectedRootClass);
            _fileContentsDictionary[@"Generated\Def1.cs"].Should().Be(ExpectedDefinedClass1);
            _fileContentsDictionary[@"Generated\Def2.cs"].Should().Be(ExpectedDefinedClass2);
        }

        [Fact(DisplayName = "DataModelGenerate generates date-time-valued properties", Skip = "https://github.com/lgolding/jschema")]
        public void GeneratesDateTimeValuedProperties()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("DateTime");

            const string Expected =
@"
using System;

namespace N
{
    /// <summary>
    /// 
    /// </summary>
    public partial class C
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime StartTime { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        private const string OutputDirectory = "Generated";

        private IFileSystem MakeFileSystem()
        {
            _mockFileSystem = new Mock<IFileSystem>();

            // The file system asserts that the output directory exists.
            _mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(OutputDirectory));

            _mockFileSystem
                .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string path, string contents) =>
                {
                    _fileContentsDictionary.Add(path, contents);
                    _mockFileSystem.Setup(fs => fs.FileExists(path)).Returns(true);
                });

            return _mockFileSystem.Object;
        }

        private static DataModelGeneratorSettings MakeSettings()
        {
            return new DataModelGeneratorSettings
            {
                NamespaceName = "N",
                RootClassName = "C",
                OutputDirectory = OutputDirectory,
                ForceOverwrite = true
            };
        }
    }
}
