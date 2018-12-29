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
            bool generateEqualityComparers)
        {
            // Each type in the schema generates a class and, optionally, an equality comparer class.
            int filesPerType = generateEqualityComparers ? 2 : 1;

            testFileSystem.Files.Count.Should().Be(expectedContentsDictionary.Count * filesPerType);

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
                    string comparerClassName = EqualityComparerGenerator.GetEqualityComparerClassName(className);
                    string comparerClassPath = TestFileSystem.MakeOutputFilePath(comparerClassName);
                    testFileSystem.Files.Should().Contain(comparerClassPath);

                    string expectedComparerClassContents = expectedContentsDictionary[className].ComparerClassContents;
                    if (expectedComparerClassContents != null)
                    {
                        testFileSystem[comparerClassPath].Should().Be(expectedComparerClassContents);
                    }
                }
            }
        }
    }
}
