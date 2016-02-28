// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.JSchema.Tests
{
    public class InferredTypeTests
    {
        [Fact(DisplayName = "InferredType works for schema with no type")]
        public void WorksForSchemaWithNoType()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.None));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.None);

            Action action = () => inferredType.GetClassName();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "InferredType works for schema of integer type")]
        public void WorksForSchemaOfIntegerType()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""integer""
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.Integer));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.Integer);

            Action action = () => inferredType.GetClassName();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "InferredType works for schema of string type")]
        public void WorksForSchemaOfStringType()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""string""
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.String));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.String);

            Action action = () => inferredType.GetClassName();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "InferredType works for schema of string type with date-time format")]
        public void WorksForSchemaOfStringTypeWithDateTimeFormat()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""string"",
  ""format"": ""date-time""
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType("System.DateTime"));
            inferredType.Kind.Should().Be(InferredTypeKind.ClassName);
            inferredType.GetClassName().Should().Be("System.DateTime");

            Action action = () => inferredType.GetJsonType();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "InferredType throws if schema reference is not fragment")]
        public void ThrowsIfSchemaReferenceIsNotFragment()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""$ref"": ""https://www.examples.com/schemas/theSchema/#/definitions/theRef"",
},");

            Action action = () => new InferredType(schema);
            action.ShouldThrow<JSchemaException>()
                .WithMessage("*" + "https://www.examples.com/schemas/theSchema/#/definitions/theRef" + "*");
        }

        [Fact(DisplayName = "InferredType throws if schema reference fragment is not subproperty of definitions")]
        public void ThrowsIfSchemaReferenceFragmentIsNotSubPropertyOfDefinitions()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""$ref"": ""#/notDefinitions/theRef"",
  ""notDefinitions"": {
    ""theRef"": {
      ""type"": ""object""
    }
  },
  ""definitions"": {
    ""theRef"": {
      ""type"": ""object""
    }
  }
},");

            Action action = () => new InferredType(schema);
            action.ShouldThrow<JSchemaException>()
                .WithMessage("*" + "#/notDefinitions/theRef" + "*");
        }

        [Fact(DisplayName = "InferredType throws if schema reference fragment definition does not exist")]
        public void ThrowsIfSchemaReferenceFragmentDefinitionDoesNotExist()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""$ref"": ""#/definitions/theRef"",
  ""definitions"": {
    ""notTheRef"": {
      ""type"": ""object""
    }
  }
},");

            Action action = () => new InferredType(schema);
            action.ShouldThrow<JSchemaException>()
                .WithMessage("*" + "theRef" + "*");
        }

        [Fact(DisplayName = "InferredType works for schema reference fragment definitions of object type")]
        public void WorksForSchemaReferenceFragmentDefinitionOfObjectType()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""$ref"": ""#/definitions/theRef"",
  ""definitions"": {
    ""theRef"": {
      ""type"": ""object""
    }
  }
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType("TheRef"));
            inferredType.Kind.Should().Be(InferredTypeKind.ClassName);
            inferredType.GetClassName().Should().Be("TheRef");

            Action action = () => inferredType.GetJsonType();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact(DisplayName = "InferredType works for schema reference fragment definitions of integer type")]
        public void WorksForSchemaReferenceFragmentDefinitionOfIntegerType()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""$ref"": ""#/definitions/theRef"",
  ""definitions"": {
    ""theRef"": {
      ""type"": ""integer"",
      ""maxValue"": 10
    }
  }
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.Integer));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.Integer);
        }

        [Fact(DisplayName = "InferredType infers string type from string enumeration")]
        public void InfersStringTypeFromStringEnumeration()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""enum"": [
    ""a"",
    ""b""
  ]
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.String));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.String);
        }

        [Fact(DisplayName = "InferredType infers integer type from integer enumeration")]
        public void InfersIntegerTypeFromIntegerEnumeration()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""enum"": [
    1,
    2
  ]
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.Integer));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.Integer);
        }

        [Fact(DisplayName = "InferredType does not infer type from mixed enumeration")]
        public void DoesNotInferTypeFromMixedEnumeration()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""enum"": [
    1,
    ""b""
  ]
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Should().Be(new InferredType(JsonType.None));
            inferredType.Kind.Should().Be(InferredTypeKind.JsonType);
            inferredType.GetJsonType().Should().Be(JsonType.None);
        }
    }
}


