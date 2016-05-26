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
            Banner();

            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    options => Run(options),
                    err => 1);
        }

        private static int Run(Options options)
        {
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

                JsonSchema schema = SchemaReader.ReadSchema(schemaText);

                var validator = new Validator(schema);

                string instanceText = File.ReadAllText(instanceFile);
                Result[] results = validator.Validate(instanceText);

                if (results.Any())
                {
                    ReportResults(instanceFile, schemaFile, results);
                }
                else
                {
                    Console.WriteLine($"Success: The file {instanceFile} is valid according to the schema {schemaFile}.");
                    returnCode = 0;
                }
            }
            catch (JsonReaderException ex)
            {
                ReportSyntaxError(ex, schemaFile);
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

        private static void ReportSyntaxError(JsonReaderException ex, string schemaFile)
        {
            Rule rule = RuleFactory.GetRuleFromErrorNumber(ErrorNumber.SyntaxError);
            var result = new Result
            {
                RuleId = rule.Id,
                Level = ResultLevel.Error,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri(schemaFile, UriKind.RelativeOrAbsolute),
                            Region = new Region
                            {
                                StartLine = ex.LineNumber,
                                StartColumn = ex.LinePosition
                            }
                        }
                    }
                },

                FormattedRuleMessage = new FormattedRuleMessage
                {
                    FormatId = RuleFactory.DefaultMessageFormatId,
                    Arguments = new List<string>
                    {
                        ex.Path,
                        ex.Message
                    }
                }
            };

            ReportResult(result);
        }

        private static void ReportInvalidSchemaErrors(SchemaValidationException ex, string schemaFile)
        {
            Console.Error.WriteLine($"Error: The schema file {schemaFile} is not valid.");
            foreach (Result result in ex.Results)
            {
                result.Locations.First().ResultFile.Uri = new Uri(schemaFile, UriKind.RelativeOrAbsolute);

                ReportResult(result);
            }
        }

        private static void ReportResults(
            string instanceFile,
            string schemaFile,
            Result[] results)
        {
            Console.Error.WriteLine($"Error: The file {instanceFile} is not valid according to the schema {schemaFile}.");
            foreach (Result result in results)
            {
                result.Locations.First().ResultFile.Uri = new Uri(instanceFile, UriKind.RelativeOrAbsolute);

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
