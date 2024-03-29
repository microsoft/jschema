﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using FluentAssertions;
using Microsoft.Json.Schema.ToDotNet.CommandLine;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    public class CommandLineTests
    {
        private const string argsStringBase = "--schema-name Sarif --schema-file-path \"sarif.json\" --output-directory \"Autogenerated\" --namespace-name Microsoft.CodeAnalysis.Sarif --root-class-name SarifLog";

        private class GenerateJsonIntegerAsTestCase
        {
            internal string InputGenerateJsonIntegerAs { get; }
            internal string ArgsString { get; }
            internal GenerateJsonIntegerOption? ExpectedGenerateJsonIntegerAs { get; }
            internal string ExpectedErrorParameter { get; }

            internal GenerateJsonIntegerAsTestCase(string inputGenerateJsonIntegerAs, GenerateJsonIntegerOption? expectedGenerateJsonIntegerAs, string expectedErrorParameter)
            {
                InputGenerateJsonIntegerAs = inputGenerateJsonIntegerAs;
                ArgsString = inputGenerateJsonIntegerAs == null ? argsStringBase : argsStringBase + " --generate-json-integer-as=" + inputGenerateJsonIntegerAs;
                ExpectedGenerateJsonIntegerAs = expectedGenerateJsonIntegerAs;
                ExpectedErrorParameter = expectedErrorParameter;
            }
        }

        private class GenerateJsonNumberAsTestCase
        {
            internal string InputGenerateJsonNumberAs { get; }
            internal string ArgsString { get; }
            internal GenerateJsonNumberOption? ExpectedGenerateJsonNumberAs { get; }
            internal string ExpectedErrorParameter { get; }

            internal GenerateJsonNumberAsTestCase(string inputGenerateJsonNumberAs, GenerateJsonNumberOption? expectedGenerateJsonNumberAs, string expectedErrorParameter)
            {
                InputGenerateJsonNumberAs = inputGenerateJsonNumberAs;
                ArgsString = inputGenerateJsonNumberAs == null ? argsStringBase : argsStringBase + " --generate-json-number-as=" + inputGenerateJsonNumberAs;
                ExpectedGenerateJsonNumberAs = expectedGenerateJsonNumberAs;
                ExpectedErrorParameter = expectedErrorParameter;
            }
        }

        [Fact]
        public void CorrectlyParseGenerateJsonIntegerAs()
        {
            var testCases = new List<GenerateJsonIntegerAsTestCase>()
            {
                new GenerateJsonIntegerAsTestCase(null, (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.Int, null),
                new GenerateJsonIntegerAsTestCase("Int", (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.Int, null),
                new GenerateJsonIntegerAsTestCase("int", (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.Int, null),
                new GenerateJsonIntegerAsTestCase("INT", (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.Int, null),
                new GenerateJsonIntegerAsTestCase("long", (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.Long, null),
                new GenerateJsonIntegerAsTestCase("biginteger", (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.BigInteger, null),
                new GenerateJsonIntegerAsTestCase("auto", (GenerateJsonIntegerOption?)GenerateJsonIntegerOption.Auto, null),
                new GenerateJsonIntegerAsTestCase("unknown", null, "generate-json-integer-as")
            };

            var builder = new StringBuilder();

            foreach (GenerateJsonIntegerAsTestCase testCase in testCases)
            {
                var args = testCase.ArgsString.Split(' ');
                var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true).ParseArguments<Options>(args)
                    .MapResult(
                    options =>
                    {
                        if (testCase.ExpectedGenerateJsonIntegerAs == null
                        || testCase.ExpectedErrorParameter != null
                        || options.GenerateJsonIntegerAs != testCase.ExpectedGenerateJsonIntegerAs)
                        {
                            builder.AppendLine($"\u2022 {testCase.ArgsString}");
                        }
                        return true;
                    },
                    err =>
                    {
                        var allErrors = err.ToList();
                        if (testCase.ExpectedGenerateJsonIntegerAs != null
                        || testCase.ExpectedErrorParameter == null
                        || allErrors.Count != 1
                        || !(allErrors[0] is BadFormatConversionError)
                        || ((NamedError)allErrors[0]).NameInfo.NameText != testCase.ExpectedErrorParameter)
                        {
                            builder.AppendLine($"\u2022 {testCase.ArgsString}");
                        }
                        return true;
                    });
            }
            builder.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{builder}");
        }

        [Fact]
        public void CorrectlyParseGenerateJsonNumberAs()
        {
            var testCases = new List<GenerateJsonNumberAsTestCase>()
            {
                new GenerateJsonNumberAsTestCase(null, (GenerateJsonNumberOption?)GenerateJsonNumberOption.Double, null),
                new GenerateJsonNumberAsTestCase("double", (GenerateJsonNumberOption?)GenerateJsonNumberOption.Double, null),
                new GenerateJsonNumberAsTestCase("float", (GenerateJsonNumberOption?)GenerateJsonNumberOption.Float, null),
                new GenerateJsonNumberAsTestCase("decimal", (GenerateJsonNumberOption?)GenerateJsonNumberOption.Decimal, null),
                new GenerateJsonNumberAsTestCase("unknown", null, "generate-json-number-as"),
                new GenerateJsonNumberAsTestCase("string", null, "generate-json-number-as")
            };

            var builder = new StringBuilder();

            foreach (GenerateJsonNumberAsTestCase testCase in testCases)
            {
                var args = testCase.ArgsString.Split(' ');
                var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true).ParseArguments<Options>(args)
                    .MapResult(
                    options =>
                    {
                        if (testCase.ExpectedGenerateJsonNumberAs == null
                        || testCase.ExpectedErrorParameter != null
                        || options.GenerateJsonNumberAs != testCase.ExpectedGenerateJsonNumberAs)
                        {
                            builder.AppendLine($"\u2022 {testCase.ArgsString}");
                        }
                        return true;
                    },
                    err =>
                    {
                        var allErrors = err.ToList();
                        if (testCase.ExpectedGenerateJsonNumberAs != null
                        || testCase.ExpectedErrorParameter == null
                        || allErrors.Count != 1
                        || !(allErrors[0] is BadFormatConversionError)
                        || ((NamedError)allErrors[0]).NameInfo.NameText != testCase.ExpectedErrorParameter)
                        {
                            builder.AppendLine($"\u2022 {testCase.ArgsString}");
                        }
                        return true;
                    });
            }
            builder.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{builder}");
        }
    }
}
