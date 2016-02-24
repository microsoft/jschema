// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;

namespace Microsoft.JSchema.Tests
{
    /// <summary>
    /// Wrapper around the mock file system used in the unit tests.
    /// </summary>
    internal class TestFileSystem
    {
        internal const string OutputDirectory = "Generated";

        private readonly Dictionary<string, string> _fileContentsDictionary;
        private readonly Mock<IFileSystem> _mockFileSystem;

        internal TestFileSystem()
        {

            _mockFileSystem = new Mock<IFileSystem>();

            // The file system asserts that the output directory exists.
            _mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns((string s) => s.Equals(OutputDirectory));

            // The file system remembers any contents written to it.
            _fileContentsDictionary = new Dictionary<string, string>();

            _mockFileSystem
                .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string path, string contents) =>
                {
                    _fileContentsDictionary.Add(path, contents);
                    _mockFileSystem.Setup(fs => fs.FileExists(path)).Returns(true);
                });
        }

        internal IFileSystem FileSystem => _mockFileSystem.Object;

        internal Mock<IFileSystem> Mock => _mockFileSystem;

        internal IList<string> Files => _fileContentsDictionary.Keys.ToList();

        internal string this[string path]
        {
            get
            {
                return _fileContentsDictionary[path];
            }
        }

        internal static string MakeOutputFilePath(string fileNameStem)
        {
            return $"{OutputDirectory}\\{fileNameStem}.cs";
        }
    }
}
