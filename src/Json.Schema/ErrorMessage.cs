// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Json.Schema
{
    internal static class ErrorMessage
    {
        private const string ErrorCodeFormat = "JS{0:D4}";

        private static readonly IDictionary<ErrorNumber, string> s_errorNumberToMessageDictionary = new Dictionary<ErrorNumber, string>
        {
            [ErrorNumber.NotAString] = Resources.ErrorNotAString,
            [ErrorNumber.InvalidAdditionalPropertiesType] = Resources.ErrorInvalidAdditionalProperties,
            [ErrorNumber.WrongType] = Resources.ErrorWrongType,
            [ErrorNumber.RequiredPropertyMissing] = Resources.ErrorRequiredPropertyMissing,
            [ErrorNumber.TooFewArrayItems] = Resources.ErrorTooFewArrayItems,
            [ErrorNumber.TooManyArrayItems] = Resources.ErrorTooManyArrayItems,
            [ErrorNumber.AdditionalPropertiesProhibited] = Resources.ErrorAdditionalPropertiesProhibited,
        };

        internal static string Format(
            int lineNumber,
            int linePosition,
            ErrorNumber errorNumber,
            params object[] args)
        {
            string messageFormat = s_errorNumberToMessageDictionary[errorNumber];
            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            string errorCode = string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);

            string fullMessage = string.Format(
                CultureInfo.CurrentCulture,
                Resources.ErrorWithLineInfo,
                lineNumber,
                linePosition,
                errorCode,
                message);

            return fullMessage;
        }
    }
}
