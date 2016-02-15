// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace MountBaker.JSchema.Tests
{
    public class ClassGeneratorTests
    {
        [Fact]
        public void ThrowsIfOutputDirectoryExists()
        {
            IFileSystem fileSystem = MakeFileSystem();

            Action action = () => ClassGenerator.Generate(new JsonSchema(), new ClassGeneratorSettings(), fileSystem);
            action.ShouldThrow<JSchemaException>();
        }

        [Fact]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            IFileSystem fileSystem = MakeFileSystem();

            var settings = new ClassGeneratorSettings
            {
                OutputDirectory = ClassGeneratorSettings.DefaultOutputDirectory + "x"
            };

            Action action = () => ClassGenerator.Generate(new JsonSchema(), settings, fileSystem);
            action.ShouldNotThrow();
        }

        private IFileSystem MakeFileSystem()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(ClassGeneratorSettings.DefaultOutputDirectory));

            return mockFileSystem.Object;
        }
    }
}
