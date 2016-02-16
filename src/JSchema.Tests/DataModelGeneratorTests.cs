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

            Action action = () => DataModelGenerator.Generate(new JsonSchema(), new DataModelGeneratorSettings(), fileSystem);
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

            Action action = () => DataModelGenerator.Generate(new JsonSchema(), settings, fileSystem);
            action.ShouldNotThrow();
        }

        [Fact]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            IFileSystem fileSystem = MakeFileSystem();
            DataModelGeneratorSettings settings = MakeSettings();

            settings.ForceOverwrite = true;

            Action action = () => DataModelGenerator.Generate(new JsonSchema(), settings, fileSystem);
            action.ShouldNotThrow();
        }

        private static IFileSystem MakeFileSystem()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            // The file system asserts that the default output directory exists.
            mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(DataModelGeneratorSettings.DefaultOutputDirectory));

            return mockFileSystem.Object;
        }

        private static DataModelGeneratorSettings MakeSettings()
        {
            return new DataModelGeneratorSettings
            {
                NamespaceName = "N",
                RootClassName = "C"
            };
        }
    }
}
