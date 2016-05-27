// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema.Sarif;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema.JsonSchemaValidator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    options => Run(options),
                    err => 1);
        }

        private static int Run(Options options)
        {
            Banner();

            int exitCode = 1;

            DateTime start = DateTime.Now;
            exitCode = Validate(options.InstanceFilePath, options.SchemaFilePath);

            if (exitCode == 0)
            {
                TimeSpan elapsedTime = DateTime.Now - start;

                // Tool notification
                Console.WriteLine(
                    string.Format(CultureInfo.CurrentCulture, Resources.ElapsedTime, elapsedTime));
            }

            return exitCode;
        }

        private static int Validate(string instanceFile, string schemaFile)
        {
            int returnCode = 1;

            try
            {
                string schemaText = File.ReadAllText(schemaFile);

                JsonSchema schema = SchemaReader.ReadSchema(schemaText, schemaFile);

                var validator = new Validator(schema);

                string instanceText = File.ReadAllText(instanceFile);
                Result[] results = validator.Validate(instanceText, instanceFile);

                if (results.Any())
                {
                    ReportResults(instanceFile, schemaFile, results);
                }
                else
                {
                    Console.WriteLine(Resources.Success);
                    returnCode = 0;
                }
            }
            catch (JsonSyntaxException ex)
            {
                ReportResult(ex.Result);
            }
            catch (SchemaValidationException ex)
            {
                ReportInvalidSchemaErrors(ex, schemaFile);
            }
            catch (Exception ex)
            {
                // Tool notification
                Console.Error.WriteLine(ex.Message);
            }

            return returnCode;
        }

        private static void ReportInvalidSchemaErrors(SchemaValidationException ex, string schemaFile)
        {
            foreach (Result result in ex.Results)
            {
                result.SetAnalysisTargetUri(schemaFile);

                ReportResult(result);
            }
        }

        private static void ReportResults(
            string instanceFile,
            string schemaFile,
            Result[] results)
        {
            foreach (Result result in results)
            {
                result.SetAnalysisTargetUri(instanceFile);

                ReportResult(result);
            }
        }

        private static void ReportResult(Result result)
        {
            Console.Error.WriteLine(
                result.FormatForVisualStudio(
                    RuleFactory.GetRuleFromRuleId(result.RuleId)));
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
