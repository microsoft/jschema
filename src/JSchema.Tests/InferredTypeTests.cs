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

        [Fact(DisplayName = "InferredType infers integer type")]
        public void InfersIntegerType()
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

        [Fact(DisplayName = "InferredType infers string type")]
        public void InfersStringType()
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

        [Fact(DisplayName = "InferredType DateTime type from string type with date-time format")]
        public void InfersDateTimeTypeFromStringTypeWithDateTimeFormat()
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

        [Fact(DisplayName = "InferredType throws if $ref is not fragment")]
        public void ThrowsIfRefIsNotFragment()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""$ref"": ""https://www.examples.com/schemas/theSchema/#/definitions/theRef"",
},");

            Action action = () => new InferredType(schema);
            action.ShouldThrow<JSchemaException>()
                .WithMessage("*" + "https://www.examples.com/schemas/theSchema/#/definitions/theRef" + "*");
        }

        [Fact(DisplayName = "InferredType throws if $ref fragment is not subproperty of definitions")]
        public void ThrowsIfRefFragmentIsNotSubPropertyOfDefinitions()
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

        [Fact(DisplayName = "InferredType throws if $ref fragment definition does not exist")]
        public void ThrowsIfRefFragmentDefinitionDoesNotExist()
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

        [Fact(DisplayName = "InferredType infers class name from $ref to object type")]
        public void InfersClassNameFromRefToObjectType()
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

        [Fact(DisplayName = "InferredType infers integer from $ref to integer type")]
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

        [Fact(DisplayName = "InferredType infers array of integer from items $ref")]
        public void InfersArrayOfIntegerFromItemsRef()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/definitions/itemType""
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""integer""
    }
  }
},");

            InferredType inferredType = new InferredType(schema);

            inferredType.Kind.Should().Be(InferredTypeKind.Array);
            inferredType.GetItemType().GetJsonType().Should().Be(JsonType.Integer);
        }

        // TODO: InfersArrayOfClassType
        // TODO: InfersArrayOfEnumType
        // TODO: InfersArrayOfInterfaceType
        // TODO: InfersArrayOfArrayOfString
        // TODO: InfersArrayOfArrayOfClassType (this is actually the case I need for SARIF).
        // TODO: Corresponding tests for inline items schemas.
        // TODO: Add unit test for $ref when there are no definitions (specific error message for this)
    }
}


