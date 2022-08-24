﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using CommandLine;
using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.CommandLine;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    public class CommandLineTests
    {
        [Fact]
        public void CorrectlyParseGenerateIntegerAs()
        {
            (string argsString, GenerateIntegerOption? expectedGenerateIntegerAs, string expectedErrorParameter)[] testCases = new[]
            {
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog", (GenerateIntegerOption?)GenerateIntegerOption.Int, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=Int", (GenerateIntegerOption?)GenerateIntegerOption.Int, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=int", (GenerateIntegerOption?)GenerateIntegerOption.Int, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=INT", (GenerateIntegerOption?)GenerateIntegerOption.Int, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=iNT", (GenerateIntegerOption?)GenerateIntegerOption.Int, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=long", (GenerateIntegerOption?)GenerateIntegerOption.Long, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=biginteger", (GenerateIntegerOption?)GenerateIntegerOption.BigInteger, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=auto", (GenerateIntegerOption?)GenerateIntegerOption.Auto, null),
                ("--schema-name Sarif --schema-file-path \"C:\\sarif.json\" --output-directory \"C:\\Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog --generate-integer-as=unknown", (GenerateIntegerOption?)null, "generate-integer-as"),
            };

            foreach ((string argsString, GenerateIntegerOption? expectedGenerateIntegerAs, string expectedErrorParameter) testCase in testCases)
            {
                var args = testCase.argsString.Split(' ');
                var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true).ParseArguments<Options>(args)
                    .MapResult(
                    options =>
                    {
                        testCase.expectedGenerateIntegerAs.Should().NotBeNull();
                        testCase.expectedErrorParameter.Should().BeNull();
                        options.GenerateIntegerAs.Should().Be(testCase.expectedGenerateIntegerAs);
                        return true;
                    },
                    err =>
                    {
                        testCase.expectedGenerateIntegerAs.Should().BeNull();
                        testCase.expectedErrorParameter.Should().NotBeNull();
                        var allErrors = err.ToList();
                        allErrors.Should().HaveCount(1);
                        allErrors[0].Should().BeOfType(typeof(BadFormatConversionError));
                        ((NamedError)allErrors[0]).NameInfo.NameText.Should().Be(testCase.expectedErrorParameter);
                        return true;
                    });
            }
        }
    }
}
