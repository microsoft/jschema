// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Validates a JSON instance against a schema.
    /// </summary>
    public class Validator
    {
        const string ErrorCodeFormat = "JSV{0:D4}";

        private static readonly IDictionary<ValidationErrorNumber, string> s_errorCodeToMessageDictionary = new Dictionary<ValidationErrorNumber, string>
        {
            [ValidationErrorNumber.WrongTokenType] = Resources.ErrorWrongTokenType
        };

        private readonly Stack<JsonSchema> _schemas;
        private IList<string> _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        /// <param name="schema">
        /// The JSON schema to use for validation.
        /// </param>
        public Validator(JsonSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            _schemas = new Stack<JsonSchema>();
            _schemas.Push(schema);
        }

        public string[] Validate(string instanceText)
        {
            _messages = new List<string>();

            using (var reader = new StringReader(instanceText))
            {
                JToken token = JToken.ReadFrom(new JsonTextReader(reader));
                JsonSchema schema = _schemas.Peek();

                ValidateToken(token, schema);
            }

            return _messages.ToArray();
        }
        private void ValidateToken(JToken token, JsonSchema schema)
        {
            // Check that the token is of the correct type, but allow an integer where a
            // "number" was specified.
            if (token.Type != schema.Type
                && !(token.Type == JTokenType.Integer && schema.Type == JTokenType.Float))
            {
                AddMessage(token, ValidationErrorNumber.WrongTokenType, schema.Type, token.Type);
                return;
            }

            switch (schema.Type)
            {
                case JTokenType.Boolean:
                    break;

                default:
                    break;
            }
        }

        private void AddMessage(JToken token, ValidationErrorNumber errorCode, params object[] args)
        {
            IJsonLineInfo lineInfo = token;

            _messages.Add(
                FormatMessage(lineInfo.LineNumber, lineInfo.LinePosition, errorCode, args));
        }

        // We factor out this method and make it internal to allow unit tests to easily
        // compare the messages produced by the validator with the expected messages.
        internal static string FormatMessage(
            int lineNumber,
            int linePosition,
            ValidationErrorNumber errorNumber,
            params object[] args)
        {
            string messageFormat = s_errorCodeToMessageDictionary[errorNumber];
            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            string errorCode = string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);

            string fullMessage = string.Format(
                CultureInfo.CurrentCulture,
                Resources.ErrorWithLocation,
                lineNumber,
                linePosition,
                errorCode,
                message);

            return fullMessage;
        }
    }
}
