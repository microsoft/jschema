// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MountBaker.JSchema.Tests;
using Xunit;

namespace MountBaker.JSchema.Generator.Tests
{
    public class DataModelGeneratorTests
    {
        private readonly IFileSystem _fileSystem;
        private readonly DataModelGeneratorSettings _settings;
        private readonly Dictionary<string, string> _fileContentsDictionary;

        public DataModelGeneratorTests()
        {
            _fileSystem = MakeFileSystem();
            _settings = MakeSettings();
            _fileContentsDictionary = new Dictionary<string, string>();
        }

        [Fact]
        public void ThrowsIfOutputDirectoryExists()
        {
            _settings.ForceOverwrite = false;

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            // ... and the message should mention the output directory.
            action.ShouldThrow<JSchemaException>().WithMessage($"*{OutputDirectory}*");
        }

        [Fact]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            // Use a directory name other than the default. The mock file system believes
            // that only the default directory exists.
            _settings.OutputDirectory = _settings.OutputDirectory + "x";

            string jsonText = TestUtil.ReadTestDataFile("Basic");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldNotThrow();
        }

        [Fact]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            // This is the default from MakeSettings; restated here for explicitness.
            _settings.ForceOverwrite = true;

            string jsonText = TestUtil.ReadTestDataFile("Basic");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            var generator = new DataModelGenerator(_settings, _fileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldNotThrow();
        }

        [Fact]
        public void ThrowsIfRootSchemaIsNotOfTypeObject()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            string jsonText = TestUtil.ReadTestDataFile("NotAnObject");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            Action action = () => generator.Generate(schema);

            // ... and the message should mention what the root type actually was.
            action.ShouldThrow<JSchemaException>().WithMessage("*number*");
        }

        [Fact]
        public void GeneratesPropertiesWithBuiltInTypes()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            string jsonText = TestUtil.ReadTestDataFile("Properties");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            generator.CreateFile(_settings.RootClassName, schema);

            string expected =
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
            _fileContentsDictionary[@"Generated\C.cs"].Should().Be(expected);
        }

        [Fact]
        public void GeneratesXmlCommentToHoldPropertyDescription()
        {
            var generator = new DataModelGenerator(_settings, _fileSystem);

            string jsonText = TestUtil.ReadTestDataFile("PropertyDescription");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            string expected =
@"namespace N
{
    public partial class C
    {
        /// <summary>An example property.</summary>
        public string ExampleProp { get; set; }
    }
}";

            string actual = generator.CreateFileText(_settings.RootClassName, schema);
            actual.Should().Be(expected);
        }

        private const string OutputDirectory = "Generated";

        private IFileSystem MakeFileSystem()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            // The file system asserts that the output directory exists.
            mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(OutputDirectory));

            mockFileSystem
                .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string path, string contents) =>
                {
                    _fileContentsDictionary.Add(path, contents);
                });

            return mockFileSystem.Object;
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
