// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema.JsonSchemaValidator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Banner();

            int exitCode = 1;

            if (args.Length == 2)
            {
                DateTime start = DateTime.Now;
                exitCode = Validate(args[0], args[1]);

                if (exitCode == 0)
                {
                    TimeSpan elapsedTime = DateTime.Now - start;

                    // Tool notification
                    Console.WriteLine(
                        string.Format(CultureInfo.CurrentCulture, Resources.ElapsedTime, elapsedTime));
                }
            }
            else
            {
                Console.Error.WriteLine("usage: JsonSchemaValidator <instanceFile> <schemaFile>");
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
                string[] errorMessages = validator.Validate(instanceText);

                if (errorMessages.Any())
                {
                    ReportErrors(instanceFile, schemaFile, errorMessages);
                }
                else
                {
                    Console.WriteLine($"Success: The file {instanceFile} is valid according to the schema {schemaFile}.");
                    returnCode = 0;
                }
            }
            catch (JsonReaderException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            catch (InvalidSchemaException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                // Tool notification
                Console.Error.WriteLine(ex.Message);
            }

            return returnCode;
        }

        private static void ReportErrors(
            string instanceFile,
            string schemaFile,
            string[] errorMessages)
        {
            Console.Error.WriteLine($"Error: The file {instanceFile} is not valid according to the schema {schemaFile}.");
            foreach (string errorMessage in errorMessages)
            {
                Console.Error.WriteLine(errorMessage);
            }
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
