// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema.Validation
{    
    public static class RuleExtensions
    {
        public static Result SetResultFile(this Result result, string filePath)
        {
            // For now, I have to make the URI absolute. Once https://github.com/Microsoft/sarif-sdk/issues/308
            // is fixed, I won't have to do this. I'll just set uriKind appropriately,
            // according to IsPathRooted.
            UriKind uriKind = UriKind.Absolute;
            if (!Path.IsPathRooted(filePath))
            {
                uriKind = UriKind.Relative;
                filePath = Path.Combine(Environment.CurrentDirectory, filePath);
            }

            foreach (Location location in result.Locations)
            {
                if (location.PhysicalLocation == null)
                {
                    location.PhysicalLocation = new PhysicalLocation();
                }

                location.PhysicalLocation.FileLocation = new FileLocation
                {
                    Uri = new Uri(filePath, uriKind)
                };
            }

            return result;
        }
    }

    public static class JsonSyntaxErrorExceptionExtensions
    {
        public static Result ToSarifResult(this JsonSyntaxException ex)
        {
            string fileName = ex.FileName;
            JsonReaderException jsonReaderException = ex.JsonReaderException;

            Rule rule = RuleFactory.GetRuleFromErrorNumber(ErrorNumber.SyntaxError);

            return new Result
            {
                RuleId = rule.Id,
                Level = rule.Configuration.DefaultLevel.ToLevel(),
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            FileLocation = new FileLocation
                            {
                                Uri = new Uri(fileName, UriKind.RelativeOrAbsolute)
                            },
                            Region = new Region
                            {
                                StartLine = jsonReaderException.LineNumber,
                                StartColumn = jsonReaderException.LinePosition
                            }
                        }
                    }
                },

                Message = new Message
                {
                    MessageId = RuleFactory.DefaultRuleMessageId,
                    Arguments = new List<string>
                    {
                        jsonReaderException.Path,
                        jsonReaderException.Message
                    }
                },
            };
        }
    }

    public static class RuleConfigurationDefaultLevelExtensions
    {
        public static ResultLevel ToLevel(this RuleConfigurationDefaultLevel ruleConfigurationDefaultLevel)
        {
            switch (ruleConfigurationDefaultLevel)
            {
                case RuleConfigurationDefaultLevel.Note:
                    return ResultLevel.Note;

                case RuleConfigurationDefaultLevel.None:
                case RuleConfigurationDefaultLevel.Warning:
                    return ResultLevel.Warning;

                case RuleConfigurationDefaultLevel.Error:
                    return ResultLevel.Error;

                case RuleConfigurationDefaultLevel.Open:
                    return ResultLevel.Open;

                default:
                    throw new ArgumentException("Invalid value: " + ruleConfigurationDefaultLevel, nameof(ruleConfigurationDefaultLevel));
            }
        }
    }
}
