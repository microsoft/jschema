// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.JSchema.Generator;
using Newtonsoft.Json;

namespace Microsoft.JSchema.DataModelGeneratorTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Banner();

            Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    options => Run(options),
                    err => 1);
        }

        private static int Run(Options options)
        {
            int exitCode = 1;

            try
            {
                string jsonText = File.ReadAllText(options.SchemaFilePath);
                JsonSchema schema = SchemaReader.ReadSchema(jsonText);

                Dictionary<UriOrFragment, ImmutableArray<CodeGenHint>> hintDictionary = null;
                if (options.CodeGenHintsPath != null)
                {
                    string hintDictionaryText = File.ReadAllText(options.CodeGenHintsPath);
                    hintDictionary =
                      JsonConvert.DeserializeObject<Dictionary<UriOrFragment, ImmutableArray<CodeGenHint>>>(hintDictionaryText);
                }

                DataModelGeneratorSettings settings = new DataModelGeneratorSettings
                {
                    OutputDirectory = options.OutputDirectory,
                    ForceOverwrite = options.ForceOverwrite,
                    NamespaceName = options.NamespaceName,
                    RootClassName = options.RootClassName,
                    CopyrightFilePath = options.CopyrightFilePath,
                    HintDictionary = hintDictionary
                };

                new DataModelGenerator(settings).Generate(schema);

                exitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.Error, ex.Message));
            }

            return exitCode;
        }

        private static void Banner()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            IEnumerable<Attribute> attributes = entryAssembly.GetCustomAttributes();

            var titleAttribute = attributes.Single(a => a is AssemblyTitleAttribute) as AssemblyTitleAttribute;
            string programName = titleAttribute.Title;

            string version = entryAssembly.GetName().Version.ToString();

            var copyrightAttribute = attributes.Single(a => a is AssemblyCopyrightAttribute) as AssemblyCopyrightAttribute;
            string copyright = copyrightAttribute.Copyright;

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.Banner, programName, version));
            Console.WriteLine(copyright);
            Console.WriteLine();
        }
    }
}
