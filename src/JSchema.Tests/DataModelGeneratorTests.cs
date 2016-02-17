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

            generator.CreateFile(_settings.RootClassName, schema);

            const string Expected =
@"namespace N
{
    public partial class C
    {
        public string StringProp { get; set; }
        public double NumberProp { get; set; }
        public bool BooleanProp { get; set; }
        public int IntegerProp { get; set; }
    }
}";
            // This particular test shows not only that the correct text was produced,
            // but that it was written to the expected path. Subsequent tests will
            // just verify the text.
            _fileContentsDictionary.Keys.Should().OnlyContain(key => key.Equals(@"Generated\C.cs"));
            _fileContentsDictionary[@"Generated\C.cs"].Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates array-valued property")]
        public void GeneratesArrayValuedProperty()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Array");

            const string Expected =
@"namespace N
{
    public partial class C
    {
        public object ExampleProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema);
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
    public partial class C
    {
        /// <summary>An example property.</summary>
        public string ExampleProp { get; set; }
    }
}";

            string actual = generator.CreateFileText(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates copyright notice")]
        public void GeneratesCopyrightAtTopOfFile()
        {
            const string CopyrightNotice =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.
";

            var generator = new DataModelGenerator(_settings, _fileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.

namespace N
{
    public partial class C
    {
        /// <summary>An example property.</summary>
        public string ExampleProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema, CopyrightNotice);
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
    public partial class C
    {
        public bool BooleanEnumProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema, null);
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
    public partial class C
    {
        public int IntegerEnumProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema, null);
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
    public partial class C
    {
        public object MixedEnumProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema, null);
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
    public partial class C
    {
        public double NumberEnumProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema, null);
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
    public partial class C
    {
        public string StringEnumProp { get; set; }
    }
}";
            string actual = generator.CreateFileText(schema, null);
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
