// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Validates a JSON instance against a schema.
    /// </summary>
    public class ValidatingJsonWalker : JsonWalker
    {
        private readonly Stack<JsonSchema> _schemas;
        private readonly IList<string> _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingJsonWalker"/> class.
        /// </summary>
        /// <param name="schema">
        /// The JSON schema to use for validation.
        /// </param>
        /// <param name="messages">
        /// List of messages describing the validation errors encountered.
        /// </param>
        public ValidatingJsonWalker(
            JsonSchema schema,
            IList<string> messages)
        {
            _schemas = new Stack<JsonSchema>();
            _schemas.Push(schema);

            _messages = messages;
        }

        public override void Walk(JToken token)
        {
            JsonSchema schema = _schemas.Peek();

            ValidateToken(token, schema);
        }

        private void ValidateToken(JToken token, JsonSchema schema)
        {
            // Check that the token is of the correct type, but allow an integer where a
            // "number" was specified.
            if (token.Type != schema.Type
                && !(token.Type == JTokenType.Integer && schema.Type == JTokenType.Float))
            {
                AddMessage(token, Resources.ErrorWrongTokenType, schema.Type, token.Type);
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

        private void AddMessage(JToken token, string format, params object[] args)
        {
            IJsonLineInfo lineInfo = token;

            _messages.Add(FormatMessage(lineInfo.LineNumber, lineInfo.LinePosition, format, args));
        }

        // We factor out this method and make it internal to allow unit tests to easily
        // compare the messages produced by the validator with the expected messages.
        internal static string FormatMessage(int lineNumber, int linePosition, string format, params object[] args)
        {
            string message = string.Format(CultureInfo.CurrentCulture, format, args);
            return string.Format(CultureInfo.CurrentCulture, Resources.ErrorWithLocation, lineNumber, linePosition, message);
        }
    }
}
