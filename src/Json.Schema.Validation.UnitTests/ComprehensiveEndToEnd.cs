// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema.Validation.CommandLine;

using Xunit;

namespace Microsoft.Json.Schema.Validation.UnitTests
{
    public class ComprehensiveEndToEnd
    {
        private const string SchemaFilePath = @"TestData\ComprehensiveSchema.json";
        private const string InstanceFilePath = @"TestData\ComprehensiveInstanceFile.json";
        private const string ExpectedOutputFilePath = @"TestData\ComprehensiveInstanceFile-validation-expected.sarif";
        private const string ActualOutputFilePath = @"TestData\ComprehensiveInstanceFile-validation-actual.sarif";

        [Fact]
        public void Validator_WhenRunOnComprehensiveSample_ProducesExpectedOutput()
        {
            int exitCode = Program.Main(new string[]
            {
                "--schema-file-path", SchemaFilePath,
                "--instance-file-path", InstanceFilePath,
                "--log-file-path", ActualOutputFilePath
            });

            exitCode.Should().Be((int)Program.ExitCode.Invalid);

            SarifLog actualLog = SarifLog.Load(ActualOutputFilePath);
            SarifLog expectedLog = SarifLog.Load(ExpectedOutputFilePath);

            // Just compare the results, because there's lots of non-deterministic stuff in the
            // invocation, driver, and artifacts.
            IList<Result> actualResults = actualLog.Runs[0].Results;
            IList<Result> expectedResults = expectedLog.Runs[0].Results;

            actualResults.Count.Should().Be(expectedResults.Count);
            for (int i = 0; i < actualResults.Count; i++)
            {
                // The validator sets the result's location URI to an absolute URI, which
                // varies across machines. So don't try to compare them.
                // Filed https://github.com/microsoft/jschema/issues/130, "Don't report validation
                // error locations or artifact locations as absolute URIs."
                actualResults[i].Locations[0].PhysicalLocation.ArtifactLocation.Uri = null;
                expectedResults[i].Locations[0].PhysicalLocation.ArtifactLocation.Uri = null;

                actualResults[i].ValueEquals(expectedResults[i]).Should().BeTrue();
            }
        }

    }
}
