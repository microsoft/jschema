// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Microsoft.Json.Schema.UnitTests
{
    public class SchemaReaderTests
    {
        public static IEnumerable<object[]> TestCases => ReaderWriter.TestCases;

        [Theory(DisplayName = "SchemaReader can read schemas")]
        [MemberData(nameof(TestCases))]
        public void CanReadSchema(string fileNameStem, JsonSchema expected)
        {
            JsonSchema actual;
            using (var reader = new StreamReader(TestUtil.GetTestDataStream(fileNameStem)))
            {
                actual = SchemaReader.ReadSchema(reader);
            }

            actual.Should().Be(expected);
        }
    }
}
