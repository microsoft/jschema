// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using FluentAssertions;

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
                "--log-file-path", "ComprehensiveInstanceDocument-validation.sarif"
            });

            exitCode.Should().Be((int)Program.ExitCode.Invalid);
        }
    }
}
