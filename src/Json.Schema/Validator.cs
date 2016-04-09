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
    /// Validates JSON documents against a JSON schema.
    /// </summary>
    public class Validator
    {
        private readonly JsonSchema _schema;
        private List<string> _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class against
        /// </summary>
        /// <param name="schema"></param>
        public Validator(JsonSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            _schema = schema;
        }

        public string[] Validate(string instanceText)
        {
            _messages = new List<string>();

            using (var reader = new StringReader(instanceText))
            {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                Validate(o, _schema);
            }

            return _messages.ToArray();
        }

        private void Validate(JObject o, JsonSchema _schema)
        {
            ValidateRequiredProperties(o, _schema);
        }

        private void ValidateRequiredProperties(JObject o, JsonSchema _schema)
        {
            if (_schema.Required == null)
            {
                return;
            }

            string[] propertyNames = o.Properties().Select(p => p.Name).ToArray();

            foreach (string requiredPropertyName in _schema.Required)
            {
                if (!propertyNames.Contains(requiredPropertyName))
                {
                    _messages.Add($"The object at path \"{o.Path}\" does not contain the required property \"{requiredPropertyName}\".");
                }
            }
        }
    }
}
