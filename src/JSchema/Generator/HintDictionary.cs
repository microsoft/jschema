// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Represents a dictionary that maps from the URI of a schema to an array of hints
    /// that apply to that schema.
    /// </summary>
    [Serializable]
    public class HintDictionary: Dictionary<string, CodeGenHint[]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HintDictionary"/> class.
        /// </summary>
        public HintDictionary() : base()
        {
        }

        /// <summary>
        /// Deserialize a <see cref="HintDictionary"/> from a string.
        /// </summary>
        /// <param name="hintsDictionaryText">
        /// A string containing the JSON serialized form of the HintDictionary.
        /// </param>
        /// <returns>
        /// The deserialized HintDictionary object.
        /// </returns>
        public static HintDictionary Deserialize(string hintsDictionaryText)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonConvert.DeserializeObject<HintDictionary>(hintsDictionaryText, settings);
        }

        protected HintDictionary(SerializationInfo info, StreamingContext context):
            base(info, context)
        {
        }
    }
}
