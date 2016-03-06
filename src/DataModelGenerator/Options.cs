// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using CommandLine;

namespace Microsoft.JSchema.DataModelGeneratorTool
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
            HelpText = "Path to directory in which the classes will be generated.",
            Required = true)]
        public string OutputDirectory { get; set; }

        [Option(
            'f',
            "force-overwrite",
            HelpText = "Overwrite files in the output directory",
            Default = false)]
        public bool ForceOverwrite { get; set; }

        [Option(
            'n',
            "namespace-name",
            HelpText = "Namespace in which the classes will be generated",
            Required = true)]
        public string NamespaceName { get; set; }

        [Option(
            'r',
            "root-class-name",
            HelpText = "Name of the class at the root of the generated object model",
            Required = true)]
        public string RootClassName { get; set; }

        [Option(
            'c',
            "copyright-file-path",
            HelpText = "Path to the file containing the copyright notice to place at the top of each file. "
                + "Can span multiple lines. "
                + "Should not include comment delimiters. ")]
        public string CopyrightFilePath { get; set; }

        [Option(
            'h',
            "hints-file-path",
            HelpText = "Path to a file containing hints that control code generation",
            Required = false)]
        public string CodeGenHintsPath { get; set; }

        [Option(
            'd',
            "generate-overrides",
            HelpText = "Generate method overrides such as Equals and GetHashCode.",
            Default = true,
            Required = false)]
        public bool GenerateOverrides { get; set; }

        [Option(
            'k',
            "generate-cloning-code",
            HelpText = "Generate code necessary to clone instances",
            Default = true,
            Required = false)]
        public bool GenerateCloningCode { get; set; }
    }
}
