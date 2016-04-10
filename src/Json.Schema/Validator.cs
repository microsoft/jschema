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
            [ValidationErrorNumber.WrongTokenType] = Resources.ErrorWrongTokenType,
            [ValidationErrorNumber.RequiredPropertyMissing] = Resources.ErrorRequiredPropertyMissing,
            [ValidationErrorNumber.TooFewArrayItems] = Resources.ErrorTooFewArrayItems
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
        private void ValidateToken(JToken jToken, JsonSchema schema)
        {
            // If the schema doesn't specify a type, anything goes.
            if (schema.Type == JTokenType.None)
            {
                return;
            }

            // Check that the token is of the correct type, but allow an integer where a
            // "number" was specified.
            if (jToken.Type != schema.Type
                && !(jToken.Type == JTokenType.Integer && schema.Type == JTokenType.Float))
            {
                AddMessage(jToken, ValidationErrorNumber.WrongTokenType, schema.Type, jToken.Type);
                return;
            }

            switch (schema.Type)
            {
                case JTokenType.Object:
                    ValidateObject((JObject)jToken, schema);
                    break;

                case JTokenType.Array:
                    ValidateArray((JArray)jToken, schema);
                    break;

                default:
                    break;
            }
        }

        private void ValidateArray(JArray jArray, JsonSchema schema)
        {
            int numItems = jArray.Count;
            if (numItems < schema.MinItems)
            {
                AddMessage(jArray, ValidationErrorNumber.TooFewArrayItems, schema.MinItems, numItems);
            }
        }

        private void ValidateObject(JObject jObject, JsonSchema schema)
        {
            List<string> propertySet = jObject.Properties().Select(p => p.Name).ToList();

            // Ensure required properties are present.
            if (schema.Required != null)
            {
                IEnumerable<string> missingProperties = schema.Required.Except(propertySet);
                foreach (string propertyName in missingProperties)
                {
                    AddMessage(jObject, ValidationErrorNumber.RequiredPropertyMissing, propertyName);
                }
            }

            // Ensure each property matches its schema.
            if (schema.Properties != null)
            {
                foreach (string propertyName in propertySet)
                {
                    JsonSchema propertySchema;
                    if (schema.Properties.TryGetValue(propertyName, out propertySchema))
                    {
                        ValidateToken(jObject.Property(propertyName).Value, propertySchema);
                    }
                }
            }
        }

        private void AddMessage(JToken jToken, ValidationErrorNumber errorCode, params object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

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
