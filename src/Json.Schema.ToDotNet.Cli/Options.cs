// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using CommandLine;

namespace Microsoft.Json.Schema.ToDotNet.CommandLine
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
            "suffix",
            HelpText = "A string to be appended to every generated type name.",
            Default = "")]
        public string TypeNameSuffix { get; set; }

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
            'e',
            "schema-name",
            HelpText = "The name of the schema",
            Required = true)]
        public string SchemaName { get; set; }

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
            'q',
            "generate-equality-comparers",
            HelpText = "Generate classes that implement IEqualityComparer<T>.",
            Default = true,
            Required = false)]
        public bool GenerateEqualityComparers { get; set; }

        [Option(
            "generate-comparers",
            HelpText = "Generate classes that implement IComparer<T>.",
            Default = true,
            Required = false)]
        public bool GenerateComparers { get; set; }

        [Option(
            'k',
            "generate-cloning-code",
            HelpText = "Generate code necessary to clone instances.",
            Required = false)]
        public bool GenerateCloningCode { get; set; }

        [Option(
            "generate-integer-as",
            HelpText = "Generate Json interger as C# type: int|long|biginteger|auto.",
            Default = GenerateIntegerOption.Int,
            Required = false)]
        public GenerateIntegerOption GenerateIntegerAs { get; set; }

        [Option(
            "seal-classes",
            HelpText = "Seal generated classes.",
            Default = false,
            Required = false)]
        public bool SealClasses { get; set; }

        [Option(
            'v',
            "virtual-members",
            HelpText = "Declare generated properties and methods as virtual.",
            Default = false,
            Required = false)]
        public bool VirtualMembers { get; set; }

        [Option(
            'p',
            "protected-init-methods",
            HelpText = "Declare generated Init methods as protected rather than private.",
            Default = false,
            Required = false)]
        public bool ProtectedInitMethods { get; set; }
    }
}
