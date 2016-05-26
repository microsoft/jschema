// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using FluentAssertions;
using Microsoft.Json.Schema.TestUtilities;
using Microsoft.Json.Schema.ToDotNet.UnitTests;

namespace Microsoft.Json.Schema.ToDotNet.Hints.UnitTests
{
    public abstract class HintTestBase : CodeGenerationTestBase
    {
        protected void RunHintTestCase(HintTestCase testCase)
        {
            string actual = null;
            Action action = () =>
            {
                Settings.HintDictionary = new HintDictionary(testCase.HintsText);
                var generator = new DataModelGenerator(Settings, TestFileSystem.FileSystem);

                JsonSchema schema = SchemaReader.ReadSchema(testCase.SchemaText, TestUtil.TestFilePath);

                actual = generator.Generate(schema);
            };

            if (testCase.ShouldThrow)
            {
                action.ShouldThrow<Exception>()
                    .WithMessage('*' + (testCase.ExpectedErrorMessage ?? string.Empty) + '*');
            }
            else
            {
                action();
                actual.Should().Be(testCase.ExpectedOutput);
            }
        }
    }
}
