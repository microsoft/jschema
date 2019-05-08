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
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.Json.Schema.Validation;

namespace Microsoft.Json.Schema.Validation.CommandLine
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

            using (var logger = new SarifLogger(
                                        options.LogFilePath,
                                        analysisTargets: new[]
                                        {
                                            options.InstanceFilePath,
                                            options.SchemaFilePath
                                        },
                                        loggingOptions: LoggingOptions.Verbose,
                                        invocationTokensToRedact: null))
            {
                DateTime start = DateTime.Now;
                exitCode = Validate(options.InstanceFilePath, options.SchemaFilePath, logger);
                TimeSpan elapsedTime = DateTime.Now - start;

                string message = string.Format(CultureInfo.CurrentCulture, ValidatorResources.ElapsedTime, elapsedTime);
                LogToolNotification(logger, message);
            }

            return exitCode;
        }

        private static int Validate(string instanceFile, string schemaFile, SarifLogger logger)
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
                    ReportResults(results, logger);
                }
                else
                {
                    LogToolNotification(logger, ValidatorResources.Success);
                    returnCode = 0;
                }
            }
            catch (JsonSyntaxException ex)
            {                
                ReportResult(ex.ToSarifResult(), logger);
            }
            catch (SchemaValidationException ex)
            {
                ReportInvalidSchemaErrors(ex, schemaFile, logger);
            }
            catch (Exception ex)
            {
                LogToolNotification(logger, ex.Message, FailureLevel.Error, ex);
            }

            return returnCode;
        }

        private static void ReportInvalidSchemaErrors(
            SchemaValidationException ex,
            string schemaFile,
            SarifLogger logger)
        {
            foreach (SchemaValidationException wrappedException in ex.WrappedExceptions)
            {
                Result result = ResultFactory.CreateResult(wrappedException.JToken, wrappedException.ErrorNumber, wrappedException.Args);
                result.SetResultFile(schemaFile);
                ReportResult(result, logger);
            }
        }

        private static void ReportResults(
            Result[] results,
            SarifLogger logger)
        {
            foreach (Result result in results)
            {
                ReportResult(result, logger);
            }
        }

        private static void ReportResult(Result result, SarifLogger logger)
        {
            ReportingDescriptor rule = RuleFactory.GetRuleFromRuleId(result.RuleId);

            Console.Error.WriteLine(
                result.FormatForVisualStudio(rule));

            logger.Log(rule, result);
        }

        private static void LogToolNotification(
            SarifLogger logger,
            string message,
            FailureLevel level = FailureLevel.Note,
            Exception ex = null)
        {
            ExceptionData exceptionData = null;
            if (ex != null)
            {
                exceptionData = new ExceptionData
                {
                    Kind = ex.GetType().FullName,
                    Message = new Message { Text = ex.Message },
                    Stack = Stack.CreateStacks(ex).FirstOrDefault()
                };
            }

            TextWriter writer = level == FailureLevel.Error ? Console.Error : Console.Out;
            writer.WriteLine(message);
            logger.LogToolNotification(new Notification
            {
                Level = level,
                Message = new Message
                {
                    Text = message
                },
                Exception = exceptionData
            });
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

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, ValidatorResources.Banner, programName, version));
            Console.WriteLine(copyright);
            Console.WriteLine();
        }
    }
}
