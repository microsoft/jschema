// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using CommandLine;

namespace Microsoft.Json.Schema.Validation.Cli
{
    internal class Options
    {
        [Option(
            's',
            "schema-file-path",
            HelpText = "Path to the JSON schema file",
            Required = true)]
        public string SchemaFilePath { get; set; }

        [Option(
            'i',
            "instance-file-path",
            HelpText = "Path to the instance file to validate.",
            Required = true)]
        public string InstanceFilePath { get; set; }

        [Option(
            'l',
            "log-file-path",
            HelpText = "Path to the log file.",
            Required = true)]
        public string LogFilePath { get; set; }
    }
}
