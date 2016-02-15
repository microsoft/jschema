// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using CommandLine;
using MountBaker.JSchema.Generator;

namespace MountBaker.JSchema.DataModelGeneratorTool
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
            'o',
            "output-directory",
            HelpText = "Path to output directory.",
            Default = DataModelGeneratorSettings.DefaultOutputDirectory)]
        public string OutputDirectory { get; set; }

        [Option(
            'f',
            "force-overwrite",
            HelpText = "Overwrite files in the output directory",
            Default = false)]
        public bool ForceOverwrite { get; set; }
    }
}
