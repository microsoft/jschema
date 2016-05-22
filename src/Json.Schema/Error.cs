// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    public class Error
    {
        internal const string RootTokenPath = "root";

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="jToken">
        /// The <see cref="JToken"/> on which the error was encountered.
        /// </param>
        /// <param name="errorNumber">
        /// The error number.
        /// </param>
        /// <param name="args">
        /// Arguments used in combination with the error number to produce a
        /// formatted error message.
        /// </param>
        public Error(JToken jToken, ErrorNumber errorNumber, params object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            Message = Format(lineInfo.LineNumber, lineInfo.LinePosition, jToken.Path, errorNumber, args);
        }

        public string Message { get; }

        private const string ErrorCodeFormat = "JS{0:D4}";

        private static readonly ImmutableDictionary<ErrorNumber, string> s_errorNumberToMessageDictionary = ImmutableDictionary.CreateRange(
            new Dictionary<ErrorNumber, string>
            {
                [ErrorNumber.NotAString] = Resources.ErrorNotAString,
                [ErrorNumber.InvalidAdditionalPropertiesType] = Resources.ErrorInvalidAdditionalProperties,
                [ErrorNumber.WrongType] = Resources.ErrorWrongType,
                [ErrorNumber.RequiredPropertyMissing] = Resources.ErrorRequiredPropertyMissing,
                [ErrorNumber.TooFewArrayItems] = Resources.ErrorTooFewArrayItems,
                [ErrorNumber.TooManyArrayItems] = Resources.ErrorTooManyArrayItems,
                [ErrorNumber.AdditionalPropertiesProhibited] = Resources.ErrorAdditionalPropertiesProhibited,
                [ErrorNumber.ValueTooLarge] = Resources.ErrorValueTooLarge,
                [ErrorNumber.ValueTooLargeExclusive] = Resources.ErrorValueTooLargeExclusive,
                [ErrorNumber.ValueTooSmall] = Resources.ErrorValueTooSmall,
                [ErrorNumber.ValueTooSmallExclusive] = Resources.ErrorValueTooSmallExclusive,
                [ErrorNumber.TooManyProperties] = Resources.ErrorTooManyProperties,
                [ErrorNumber.TooFewProperties] = Resources.ErrorTooFewProperties,
                [ErrorNumber.NotAMultiple] = Resources.ErrorNotAMultiple,
                [ErrorNumber.StringTooLong] = Resources.ErrorStringTooLong,
                [ErrorNumber.StringTooShort] = Resources.ErrorStringTooShort,
                [ErrorNumber.StringDoesNotMatchPattern] = Resources.ErrorStringDoesNotMatchPattern,
                [ErrorNumber.NotAllOf] = Resources.ErrorNotAllOf,
                [ErrorNumber.NotAnyOf] = Resources.ErrorNotAnyOf,
                [ErrorNumber.NotOneOf] = Resources.ErrorNotOneOf,
                [ErrorNumber.InvalidEnumValue] = Resources.ErrorInvalidEnumValue,
                [ErrorNumber.NotUnique] = Resources.ErrorNotUnique,
                [ErrorNumber.TooFewItemSchemas] = Resources.ErrorTooFewItemSchemas,
                [ErrorNumber.ValidatesAgainstNotSchema] = Resources.ErrorValidatesAgainstNotSchema
            });

        internal static ApplicationException CreateException(string messageFormat, params object[] messageArgs)
        {
            return new ApplicationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    messageFormat,
                    messageArgs));
        }

        internal static ApplicationException CreateException(
            int lineNumber,
            int linePosition,
            ErrorNumber errorNumber,
            params object[] args)
        {
            return new ApplicationException(
                Format(lineNumber, linePosition, null, errorNumber, args));
        }

        internal static string Format(
            int lineNumber,
            int linePosition,
            string path,
            ErrorNumber errorNumber,
            params object[] args)
        {
            string messageFormat = s_errorNumberToMessageDictionary[errorNumber];
            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            string errorCode = string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);

            string fullMessage;
            if (path != null)
            {
                path = path == string.Empty ? RootTokenPath : "'" + path + "'";

                fullMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ErrorWithLineInfoAndPath,
                    lineNumber,
                    linePosition,
                    errorCode,
                    path,
                    message);
            }
            else
            {
                fullMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ErrorWithLineInfo,
                    lineNumber,
                    linePosition,
                    errorCode,
                    message);
            }

            return fullMessage;
        }
    }
}
