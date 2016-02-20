// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Microsoft.JSchema
{
    /// <summary>
    /// Works around Json.NET's special treatment of properties named "$ref".
    /// </summary>
    /// <remarks>
    /// The property name in the schema file is "$ref", but Json.NET treats $ref specially.
    /// To work around this problem, on input, we use a regex to change "$ref" to "$$ref"
    /// after we read the schema file and before we ask Json.NET to deserialize it into a
    /// JsonSchema object. On output, we use another regex to change "$$ref" to "$ref"
    /// after we ask Json.NET to serialize the schema to a string, and before we write
    /// that string to the output stream. The regexes account for the quotes surrounding the
    /// property name and the colon that follows it.
    /// See https://github.com/lgolding/jschema/issues/20
    /// </remarks>
    internal static class RefProperty
    {
        // The patterns start with "any character that is not a quote, followed by a quote".
        // This avoids a corner case where we might mistakenly change this:
        //
        //     "My property ""$ref": "My value"
        //
        // to this:
        //
        //     "My property ""$$ref": "My value"
        //
        private static readonly string s_inputPattern = "(?<before>[^\"]\")\\$ref(?<after>\"\\s*:)";
        private static readonly string s_outputPattern = "(?<before>[^\"]\")\\$\\$ref(?<after>\"\\s*:)";

        internal static string ConvertFromInput(string jsonText)
        {
            return Regex.Replace(jsonText, s_inputPattern, "${before}$$$$ref${after}");
        }

        internal static string ConvertToOutput(string jsonText)
        {
            return Regex.Replace(jsonText, s_outputPattern, "${before}$$ref${after}");
        }
    }
}
