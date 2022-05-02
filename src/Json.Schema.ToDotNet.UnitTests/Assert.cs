// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    internal static class Assert
    {
        internal static void FileContentsMatchExpectedContents(
            TestFileSystem testFileSystem,
            IDictionary<string, ExpectedContents> expectedContentsDictionary,
            bool generateEqualityComparers,
            bool generateComparers)
        {
            // Each type in the schema generates a class and, optionally, an equality comparer class.
            int filesPerType = 1 + (generateEqualityComparers ? 1 : 0) + (generateComparers ? 1 : 0);
            int extensionClassCount = generateComparers ? 1 : 0;

            testFileSystem.Files.Count.Should().Be(expectedContentsDictionary.Count * filesPerType + extensionClassCount);

            foreach (string className in expectedContentsDictionary.Keys)
            {
                string classPath = TestFileSystem.MakeOutputFilePath(className);
                testFileSystem.Files.Should().Contain(classPath);

                string expectedClassContents = expectedContentsDictionary[className].ClassContents;
                if (expectedClassContents != null)
                {
                    testFileSystem[classPath].Should().Be(expectedClassContents);
                }

                if (generateEqualityComparers)
                {
                    string equalityComparerClassName = EqualityComparerGenerator.GetEqualityComparerClassName(className);
                    string equalityComparerClassPath = TestFileSystem.MakeOutputFilePath(equalityComparerClassName);
                    testFileSystem.Files.Should().Contain(equalityComparerClassPath);

                    string expectedComparerClassContents = expectedContentsDictionary[className].EqualityComparerClassContents;
                    if (expectedComparerClassContents != null)
                    {
                        testFileSystem[equalityComparerClassPath].Should().Be(expectedComparerClassContents);
                    }
                }

                if (generateComparers)
                {
                    string comparerClassName = ComparerCodeGenerator.GetComparerClassName(className);
                    string comparerClassPath = TestFileSystem.MakeOutputFilePath(comparerClassName);
                    string comparerExtensionsClassName = ComparerCodeGenerator.GetComparerExtensionsClassName();
                    string comparerExtensionsClassPath = TestFileSystem.MakeOutputFilePath(comparerExtensionsClassName);
                    testFileSystem.Files.Should().Contain(comparerClassPath);
                    testFileSystem.Files.Should().Contain(comparerExtensionsClassPath);

                    string expectedComparerClassContents = expectedContentsDictionary[className].ComparerClassContents;
                    if (expectedComparerClassContents != null)
                    {
                        testFileSystem[comparerClassPath].Should().Be(expectedComparerClassContents);
                    }
                    testFileSystem[comparerExtensionsClassPath].Should().Be(ExpectedContents.ComparerExtensionsClassContents);
                }
            }
        }
    }
}
