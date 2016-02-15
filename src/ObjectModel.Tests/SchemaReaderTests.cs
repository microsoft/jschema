// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace MountBaker.JSchema.ObjectModel.Tests
{
    public class SchemaReaderTests
    {
        [Fact]
        public void EmptySchema()
        {
            string jsonText = File.ReadAllText(@"TestData\Empty.schema.json");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            schema.Should().Be(new JsonSchema());
        }

        [Fact]
        public void SchemaWithOnlyV4Property()
        {
            string jsonText = File.ReadAllText(@"TestData\V4.schema.json");
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            schema.Should().Be(new JsonSchema { SchemaVersion = JsonSchema.V4Draft });
        }
    }
}
