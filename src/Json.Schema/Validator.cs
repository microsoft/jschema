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
                JToken token = JToken.ReadFrom(new JsonTextReader(reader));
                var validator = new ValidatingJsonWalker(_schema, _messages);
                validator.Walk(token);
            }

            return _messages.ToArray();
        }
    }
}
