// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.JsonSchemaValidator.Sarif
{
    public partial class ResultFactory
    {
        private const string ErrorCodeFormat = "JS{0:D4}";

        internal static Result CreateResult(JToken jToken, ErrorNumber errorNumber, object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            return CreateResult(
                lineInfo.LineNumber,
                lineInfo.LinePosition,
                jToken.Path,
                errorNumber,
                args);
        }

        internal static Result CreateResult(
            int startLine,
            int startColumn,
            string jsonPath,
            ErrorNumber errorNumber,
            params object[] args)
        {
            Rule rule = RuleFactory.GetRuleFromErrorNumber(errorNumber);

            var messageArguments = new List<string> { jsonPath };
            messageArguments.AddRange(args.Select(a => a.ToString()));

            var result = new Result
            {
                RuleId = rule.Id,
                Level = rule.DefaultLevel,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = startLine,
                                StartColumn = startColumn
                            }
                        }
                    }
                },

                FormattedRuleMessage = new FormattedRuleMessage
                {
                    FormatId = RuleFactory.DefaultMessageFormatId,
                    Arguments = messageArguments
                }
            };

            result.SetProperty("jsonPath", jsonPath);

            return result;
        }

        internal static string RuleIdFromErrorNumber(ErrorNumber errorNumber)
        {
            return string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);
        }
    }
}
