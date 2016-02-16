// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace MountBaker.JSchema.Generator.Tests
{
    public class DataModelGeneratorTests
    {
        [Fact]
        public void ThrowsIfOutputDirectoryExists()
        {
            IFileSystem fileSystem = MakeFileSystem();
            DataModelGeneratorSettings settings = MakeSettings();

            var generator = new DataModelGenerator(settings, fileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            action.ShouldThrow<JSchemaException>();
        }

        [Fact]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            IFileSystem fileSystem = MakeFileSystem();
            DataModelGeneratorSettings settings = MakeSettings();

            // Use a directory name other than the default. The mock file system believes
            // that only the default directory exists.
            settings.OutputDirectory = settings.OutputDirectory + "x";

            var generator = new DataModelGenerator(settings, fileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            action.ShouldNotThrow();
        }

        [Fact]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            IFileSystem fileSystem = MakeFileSystem();
            DataModelGeneratorSettings settings = MakeSettings();

            settings.ForceOverwrite = true;

            var generator = new DataModelGenerator(settings, fileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            action.ShouldNotThrow();
        }

        private const string OutputDirectory = "D";

        private static IFileSystem MakeFileSystem()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            // The file system asserts that the output directory exists.
            mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(OutputDirectory));

            return mockFileSystem.Object;
        }

        private static DataModelGeneratorSettings MakeSettings()
        {
            return new DataModelGeneratorSettings
            {
                NamespaceName = "N",
                RootClassName = "C",
                OutputDirectory = OutputDirectory
            };
        }
    }
}
