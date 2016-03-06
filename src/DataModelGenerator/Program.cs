// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.JSchema.Generator;

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

                HintDictionary hintDictionary = null;
                if (options.CodeGenHintsPath != null)
                {
                    if (!File.Exists(options.CodeGenHintsPath))
                    {
                        throw new ArgumentException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.ErrorHintsFileNotFound,
                                options.CodeGenHintsPath));
                    }

                    string hintDictionaryText = File.ReadAllText(options.CodeGenHintsPath);
                    hintDictionary = HintDictionary.Deserialize(hintDictionaryText);
                }

                string copyrightNotice = null;
                if (options.CopyrightFilePath != null)
                {
                    if (!File.Exists(options.CopyrightFilePath))
                    {
                        throw new ArgumentException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.ErrorCopyrightFileNotFound,
                                options.CopyrightFilePath));
                    }

                    copyrightNotice = File.ReadAllText(options.CopyrightFilePath);
                }

                DataModelGeneratorSettings settings = new DataModelGeneratorSettings
                {
                    OutputDirectory = options.OutputDirectory,
                    ForceOverwrite = options.ForceOverwrite,
                    NamespaceName = options.NamespaceName,
                    RootClassName = options.RootClassName,
                    CopyrightNotice = copyrightNotice,
                    HintDictionary = hintDictionary,
                    GenerateOverrides = options.GenerateOverrides,
                    GenerateCloningCode = options.GenerateCloningCode
                };

                new DataModelGenerator(settings).Generate(schema);

                exitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error,
                        ex.Message));
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
