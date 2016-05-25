// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.Sarif
{
    internal partial class ResultFactory
    {
        private const string ErrorCodeFormat = "JS{0:D4}";

        internal static readonly Uri TestFileUri = new Uri("file:///C:/test", UriKind.Absolute);

        internal static Result CreateResult(JToken jToken, ErrorNumber errorNumber, object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            var messageArguments = new List<string> { jToken.Path };
            messageArguments.AddRange(args.Select(a => a.ToString()));

            var result = new Result
            {
                RuleId = RuleIdFromErrorNumber(errorNumber),
                Locations = new List<Location>
                {
                    new Location
                    {
                        ResultFile = new PhysicalLocation
                        {
                            Uri = TestFileUri,
                            Region = new Region
                            {
                                StartLine = lineInfo.LineNumber,
                                StartColumn = lineInfo.LinePosition
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

            result.SetProperty("jsonPath", jToken.Path);

            return result;
        }

        internal static string RuleIdFromErrorNumber(ErrorNumber errorNumber)
        {
            return string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);
        }
    }
}
