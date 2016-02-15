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

            Action action = () => DataModelGenerator.Generate(new JsonSchema(), new DataModelGeneratorSettings(), fileSystem);
            action.ShouldThrow<JSchemaException>();
        }

        [Fact]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            IFileSystem fileSystem = MakeFileSystem();

            var settings = new DataModelGeneratorSettings
            {
                OutputDirectory = DataModelGeneratorSettings.DefaultOutputDirectory + "x"
            };

            Action action = () => DataModelGenerator.Generate(new JsonSchema(), settings, fileSystem);
            action.ShouldNotThrow();
        }

        [Fact]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            IFileSystem fileSystem = MakeFileSystem();

            var settings = new DataModelGeneratorSettings
            {
                ForceOverwrite = true
            };

            Action action = () => DataModelGenerator.Generate(new JsonSchema(), settings, fileSystem);
            action.ShouldNotThrow();
        }

        private IFileSystem MakeFileSystem()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(DataModelGeneratorSettings.DefaultOutputDirectory));

            return mockFileSystem.Object;
        }
    }
}
