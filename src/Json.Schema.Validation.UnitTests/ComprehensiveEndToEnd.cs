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
        [Fact]
        public void Validator_WhenRunOnComprehensiveSample_ProducesExpectedOutput()
        {
            int exitCode = Program.Main(new string[]
            {
                "--schema-file-path", @"TestData\ComprehensiveSchema.json",
                "--instance-file-path", @"TestData\ComprehensiveInstanceDocument.json",
                "--log-file-path", @"TestData\ComprehensiveInstanceDocument-validation-actual.sarif"
            });

            exitCode.Should().Be((int)Program.ExitCode.Invalid);

            SarifLog actualLog = SarifLog.Load(@"TestData\ComprehensiveInstanceDocument-validation-actual.sarif");
            SarifLog expectedLog = SarifLog.Load(@"TestData\ComprehensiveInstanceDocument-validation-expected.sarif");

            // Just compare the results, because there's lots of non-deterministic stuff in the
            // invocation, driver, and artifacts.
            IList<Result> actualResults = actualLog.Runs[0].Results;
            IList<Result> expectedResults = expectedLog.Runs[0].Results;

            actualResults.Count.Should().Be(expectedResults.Count);
            for (int i = 0; i < actualResults.Count; i++)
            {
                // The validator sets the result's location URI to an absolute URI, which
                // varies across machines. So don't try to compare them.
                actualResults[i].Locations[0].PhysicalLocation.ArtifactLocation.Uri = null;
                expectedResults[i].Locations[0].PhysicalLocation.ArtifactLocation.Uri = null;

                actualResults[i].ValueEquals(expectedResults[i]).Should().BeTrue();
            }
        }

    }
}
