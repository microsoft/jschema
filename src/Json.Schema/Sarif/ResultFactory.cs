// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.Sarif
{
    internal partial class ResultFactory
    {
        private const string ErrorCodeFormat = "JS{0:D4}";

        internal static Result CreateResult(JToken jToken, ErrorNumber errorNumber, object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            string messageFormat = Error.s_errorNumberToMessageDictionary[errorNumber];

            var result = new Result
            {
                RuleId = RuleIdFromErrorNumber(errorNumber),
                Locations = new List<Location>
                {
                    new Location
                    {
                        ResultFile = new PhysicalLocation
                        {
                            Uri = new Uri("file:///C:/test", UriKind.Absolute),
                            Region = new Region
                            {
                                StartLine = lineInfo.LineNumber,
                                StartColumn = lineInfo.LinePosition
                            }
                        }
                    }
                },

                Message = string.Format(CultureInfo.CurrentCulture, messageFormat, args)
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
