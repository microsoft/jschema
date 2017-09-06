﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema.JsonSchemaValidator.Sarif;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema.JsonSchemaValidator
{    
    public static class RuleExtensions
    {
        public static Result SetAnalysisTargetUri(this Result result, string filePath)
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

            result.Locations.First().AnalysisTarget.Uri = new Uri(filePath, uriKind);

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
                Level = rule.DefaultLevel,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri(fileName, UriKind.RelativeOrAbsolute),
                            Region = new Region
                            {
                                StartLine = jsonReaderException.LineNumber,
                                StartColumn = jsonReaderException.LinePosition
                            }
                        }
                    }
                },

                FormattedRuleMessage = new FormattedRuleMessage
                {
                    FormatId = RuleFactory.DefaultMessageFormatId,
                    Arguments = new List<string>
                    {
                        jsonReaderException.Path,
                        jsonReaderException.Message
                    }
                }
            };
        }
    }
}
