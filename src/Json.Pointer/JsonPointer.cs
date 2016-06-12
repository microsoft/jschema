// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Pointer
{
    /// <summary>
    /// Represents a JSON Pointer as defined in RFC 6901.
    /// </summary>
    public class JsonPointer
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPointer"/> class with the
        /// specified string value.
        /// </summary>
        /// <param name="value">
        /// The string value of the JSON Pointer.
        /// </param>
        public JsonPointer(string value)
        {
            _value = value;
            ReferenceTokens = ImmutableArray.CreateRange(Parse(value));
        }

        public ImmutableArray<string> ReferenceTokens { get; }

        public JToken Evaluate(JToken document)
        {
            JToken result = document;
            foreach (string referenceToken in ReferenceTokens)
            {
                result = Evaluate(referenceToken, result);
            }

            return result;
        }

        private JToken Evaluate(string referenceToken, JToken current)
        {
            JObject jObject = current as JObject;
            if (jObject != null)
            {
                return EvaluateObjectReference(referenceToken, jObject);
            }

            return null;
        }

        private JToken EvaluateObjectReference(string referenceToken, JObject current)
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
                        referenceToken),
                    nameof(referenceToken));
            }
        }

        private static readonly Regex s_pointerPattern = new Regex(
@"^
    (
        /
        (?<referenceToken>
            [^~/]*
            | ~0
            | ~1
        )?
    )*
$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private IEnumerable<string> Parse(string value)
        {
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
