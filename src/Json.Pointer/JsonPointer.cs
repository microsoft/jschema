// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Pointer
{
    /// <summary>
    /// Represents a JSON Pointer as defined in RFC 6901.
    /// </summary>
    public class JsonPointer
    {
        private const string TokenSeparator = "/";
        private const string EscapeCharacter = "~";
        private const char UriFragmentDelimiter = '#';

        private readonly string _value;
        private readonly JsonPointerRepresentation _representation;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPointer"/> class with the
        /// specified string value.
        /// </summary>
        /// <param name="value">
        /// The string value of the JSON Pointer.
        /// </param>
        public JsonPointer(string value)
            : this(value, JsonPointerRepresentation.Normal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPointer"/> class with the
        /// specified string value.
        /// </summary>
        /// <param name="value">
        /// The string value of the JSON Pointer.
        /// </param>
        /// <param name="representation">
        /// A value that specifies the representation of the JON Pointer.
        /// </param>
        public JsonPointer(
            string value,
            JsonPointerRepresentation representation = JsonPointerRepresentation.Normal)
        {
            _value = value;
            _representation = representation;

            ReferenceTokens = ImmutableArray.CreateRange(Parse(value));
        }

        public ImmutableArray<string> ReferenceTokens { get; }

        public JToken Evaluate(JToken document)
        {
            JToken result = document;
            StringBuilder pathBuilder = new StringBuilder();
            foreach (string referenceToken in ReferenceTokens)
            {
                string unescapedToken = referenceToken.UnescapeJsonPointer();
                result = Evaluate(unescapedToken, pathBuilder, result);
                pathBuilder.Append(TokenSeparator + referenceToken);
            }

            return result;
        }

        private JToken Evaluate(string referenceToken, StringBuilder pathBuilder, JToken current)
        {
            JObject jObject = current as JObject;
            if (jObject != null)
            {
                return EvaluateObjectReference(referenceToken, pathBuilder, jObject);
            }

            JArray jArray = current as JArray;
            if (jArray != null)
            {
                return EvaluateArrayReference(referenceToken, pathBuilder, jArray);
            }

            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ErrorNeitherObjectNorArray,
                    _value,
                    pathBuilder),
                nameof(referenceToken));
        }

        private JToken EvaluateObjectReference(string referenceToken, StringBuilder pathBuilder, JObject current)
        {
            IEnumerable<string> propertyNames = current.Properties().Select(p => p.Name);
            if (propertyNames.Contains(referenceToken))
            {
                return current.Property(referenceToken).Value;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.ErrorMissingProperty,
                        _value,
                        pathBuilder,
                        referenceToken),
                    nameof(referenceToken));
            }
        }

        private static readonly Regex s_indexPattern =
            new Regex(@"^(0|[1-9][0-9]*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private JToken EvaluateArrayReference(string referenceToken, StringBuilder pathBuilder, JArray jArray)
        {
            if (s_indexPattern.IsMatch(referenceToken))
            {
                int index = int.Parse(referenceToken, NumberStyles.None, CultureInfo.InvariantCulture);
                if (index >= jArray.Count)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.ErrorArrayIndexOutOfRange,
                            _value,
                            referenceToken,
                            pathBuilder),
                        nameof(referenceToken));
                }

                return jArray[index];
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.ErrorInvalidArrayIndex,
                        _value,
                        referenceToken),
                    nameof(referenceToken));
            }
        }

        private static readonly Regex s_pointerPattern = new Regex(
@"^
    (
        /
        (?<referenceToken>
            (
                [^~/]
                | ~0
                | ~1
             )*
        )?
    )*
$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private IEnumerable<string> Parse(string value)
        {
            if (_representation == JsonPointerRepresentation.JsonString)
            {
                // TODO: Handle control characters 0x00-0x1F.
                value = value.Replace(@"\\", @"\").Replace(@"\""", @"""");
            }
            else if (_representation == JsonPointerRepresentation.UriFragment)
            {
                if (value[0] != UriFragmentDelimiter)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.ErrorInvalidFragmentStartCharacter,
                            value),
                        nameof(value));
                }

                value = Uri.UnescapeDataString(value.Substring(1));
            }

            Match match = s_pointerPattern.Match(value);
            if (match.Success)
            {
                CaptureCollection referenceTokenCaptures = match.Groups["referenceToken"].Captures;
                var captureArray = new Capture[referenceTokenCaptures.Count];
                referenceTokenCaptures.CopyTo(captureArray, 0);
                return captureArray.Select(c => c.Value);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.ErrorInvalidJsonPointer,
                        value),
                    nameof(value));
            }
        }
    }
}
