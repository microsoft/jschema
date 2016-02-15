// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using CommandLine;
using MountBaker.JSchema.Generator;

namespace MountBaker.JSchema.DataModelGeneratorTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    options => Run(options),
                    err => 1);
        }

        private static int Run(Options options)
        {
            string jsonText = File.ReadAllText(options.SchemaFilePath);
            JsonSchema schema = SchemaReader.ReadSchema(jsonText);

            DataModelGeneratorSettings settings = new DataModelGeneratorSettings
            {
                OutputDirectory = options.OutputDirectory,
                ForceOverwrite = options.ForceOverwrite
            };

            DataModelGenerator.Generate(schema, settings);
            return 0;
        }
    }
}
