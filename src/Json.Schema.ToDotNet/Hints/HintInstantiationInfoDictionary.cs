// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a dictionary that maps from the URI of a schema to an array of
    /// <see cref="HintInstantiationInfo"/> objects which specify how to
    /// instantiate the code generation hints that apply to that schema.
    /// </summary>
    [Serializable]
    public class HintInstantiationInfoDictionary : Dictionary<string, HintInstantiationInfo[]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HintDictionary"/> class.
        /// </summary>
        public HintInstantiationInfoDictionary() : base()
        {
        }

        /// <summary>
        /// Deserialize a <see cref="HintInstantiationInfoDictionary"/> from a string.
        /// </summary>
        /// <param name="dictionaryText">
        /// A string containing the JSON serialized form of the dictionary.
        /// </param>
        /// <returns>
        /// The deserialized HintInstantiationInfoDictionary object.
        /// </returns>
        public static HintInstantiationInfoDictionary Deserialize(string dictionaryText)
        {
            return JsonConvert.DeserializeObject<HintInstantiationInfoDictionary>(dictionaryText);
        }

        protected HintInstantiationInfoDictionary(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}
