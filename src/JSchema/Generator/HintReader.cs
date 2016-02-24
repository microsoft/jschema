// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Microsoft.JSchema.Generator
{
    public static class HintReader
    {
        /// <summary>
        /// Deserialize a dictionary of code generation hints from a string.
        /// </summary>
        /// <param name="hintsText">
        /// A string containing the JSON serialized for of the hints dictionary.
        /// </param>
        /// <returns>
        /// A dictionary that maps the URI of the schema to which the hints apply
        /// to an array of hints that apply to that schema.
        /// </returns>
        public static Dictionary<UriOrFragment, CodeGenHint[]> ReadHints(string path)
        {
            return null;
        }
    }
}
