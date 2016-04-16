// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        internal const string RootObjectName = "root object";

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

                ValidateToken(token, RootObjectName, schema);
            }

            return _messages.ToArray();
        }
        private void ValidateToken(JToken jToken, string name, JsonSchema schema)
        {
            // If the schema doesn't specify a type, anything goes.
            if (schema.Type == null || schema.Type.Length == 0)
            {
                return;
            }

            // Check that the token is of the correct type, but allow an integer where a
            // "number" was specified.
            if (jToken.Type != schema.Type[0]
                && !(jToken.Type == JTokenType.Integer && schema.Type[0] == JTokenType.Float))
            {
                AddMessage(jToken, ErrorNumber.WrongType, name, schema.Type[0], jToken.Type);
                return;
            }

            switch (schema.Type[0])
            {
                case JTokenType.Integer:
                case JTokenType.Float:
                    ValidateNumber((JValue)jToken, schema);
                    break;

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

        private void ValidateNumber(JValue jValue, JsonSchema schema)
        {
            if (schema.Maximum != null)
            {
                double maximum = schema.Maximum.Value;
                double value = jValue.Type == JTokenType.Float
                    ? (double)jValue.Value
                    : (long)jValue.Value;

                if (schema.ExclusiveMaximum == true && value >= maximum)
                {
                    AddMessage(jValue, ErrorNumber.ValueTooLargeExclusive, value, maximum);
                }
                else if (value > maximum)
                {
                    AddMessage(jValue, ErrorNumber.ValueTooLarge, value, maximum);
                }
            }

            if (schema.Minimum != null)
            {
                double minimum = schema.Minimum.Value;
                double value = jValue.Type == JTokenType.Float
                    ? (double)jValue.Value
                    : (long)jValue.Value;

                if (schema.ExclusiveMinimum == true && value <= minimum)
                {
                    AddMessage(jValue, ErrorNumber.ValueTooSmallExclusive, value, minimum);
                }
                else if (value < minimum)
                {
                    AddMessage(jValue, ErrorNumber.ValueTooSmall, value, minimum);
                }
            }
        }

        private void ValidateArray(JArray jArray, JsonSchema schema)
        {
            int numItems = jArray.Count;
            if (numItems < schema.MinItems)
            {
                AddMessage(jArray, ErrorNumber.TooFewArrayItems, schema.MinItems, numItems);
            }

            if (numItems > schema.MaxItems)
            {
                AddMessage(jArray, ErrorNumber.TooManyArrayItems, schema.MaxItems, numItems);
            }
        }

        private void ValidateObject(JObject jObject, JsonSchema schema)
        {
            List<string> propertySet = jObject.Properties().Select(p => p.Name).ToList();

            if (schema.MaxProperties.HasValue &&
                propertySet.Count > schema.MaxProperties.Value)
            {
                AddMessage(jObject, ErrorNumber.TooManyProperties, schema.MaxProperties.Value, propertySet.Count);
            }

            if (schema.MinProperties.HasValue &&
                propertySet.Count < schema.MinProperties.Value)
            {
                AddMessage(jObject, ErrorNumber.TooFewProperties, schema.MinProperties.Value, propertySet.Count);
            }

            // Ensure required properties are present.
            if (schema.Required != null)
            {
                IEnumerable<string> missingProperties = schema.Required.Except(propertySet);
                foreach (string propertyName in missingProperties)
                {
                    AddMessage(jObject, ErrorNumber.RequiredPropertyMissing, propertyName);
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
                        JProperty property = jObject.Property(propertyName);
                        ValidateToken(property.Value, property.Path, propertySchema);
                    }
                }
            }

            // Does the object contain any properties not specified by the schema?
            IEnumerable<string> extraProperties = schema.Properties == null
                ? propertySet
                : propertySet.Except(schema.Properties.Keys);

            // If additional properties are not allowed, ensure there are none.
            if (!(schema.AdditionalProperties?.Allowed  == true))
            {
                foreach (string propertyName in extraProperties)
                {
                    JProperty property = jObject.Property(propertyName);
                    AddMessage(property, ErrorNumber.AdditionalPropertiesProhibited, propertyName);
                }
            }
            else
            {
                // Additional properties are allowed. If there is a schema to which they
                // must conform, ensure that they do.
                if (schema.AdditionalProperties.Schema != null)
                {
                    foreach (string propertyName in extraProperties)
                    {
                        JProperty property = jObject.Property(propertyName);
                        ValidateToken(property.Value, property.Path, schema.AdditionalProperties.Schema);
                    }
                }
            }
        }

        private void AddMessage(JToken jToken, ErrorNumber errorCode, params object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            _messages.Add(
                Error.Format(lineInfo.LineNumber, lineInfo.LinePosition, errorCode, args));
        }
    }
}
