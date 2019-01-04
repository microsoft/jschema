// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Newtonsoft defines TraceLevel inconsistently between .NET Framework and .NET Standard.
// This fixes the problem. See https://github.com/JamesNK/Newtonsoft.Json/issues/1616.
#if NET461
using TraceLevel = System.Diagnostics.TraceLevel;
#else
using TraceLevel = Newtonsoft.Json.TraceLevel;
#endif

namespace Microsoft.Json.Schema
{
    public static class DictionaryExtensions
    {
        internal static bool HasSameElementsAs<K, V>(this Dictionary<K, V> left, Dictionary<K, V> right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            // http://stackoverflow.com/questions/3804367/testing-for-equality-between-dictionaries-in-c-sharp
            return left.Count == right.Count && !left.Except(right).Any();
        }
    }

    public static class IEnumerableExtensions
    {
        public static bool HasSameElementsAs<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Count() == right.Count() && !left.Except(right).Any();
        }
    }

    internal static class UriExtensions
    {
        /// <summary>
        /// Compares two URIs, taking account of their fragments, if any.
        /// </summary>
        /// <param name="right">
        /// The first URI to compare.
        /// </param>
        /// <param name="left">
        /// The second URI to compare.
        /// </param>
        /// <returns>
        /// True if the URIs are equal, including their fragments, if any;
        /// otherwise false.
        /// </returns>
        internal static bool EqualsWithFragments(this Uri right, Uri left)
        {
            if (!right.Equals(left))
            {
                return false;
            }

            // If the URIs were equal, they were both either absolute or both
            // relative. If they were relative, the comparison took account of their
            // fragments.
            if (!right.IsAbsoluteUri)
            {
                return true;
            }

            // If they were absolute, the comparison did not take account of their
            // fragments, so we'll compare the fragments ourselves.
            return right.Fragment.Equals(left.Fragment, StringComparison.Ordinal);
        }
    }

    public static class UriOrFragmentExtensions
    {
        private static readonly Regex s_definitionRegex = new Regex(@"^#/definitions/(?<definitionName>[^/]+)$");

        public static string GetDefinitionName(this UriOrFragment reference)
        {
            Match match = s_definitionRegex.Match(reference.Fragment);
            if (!match.Success)
            {
                throw Error.CreateException(
                    Resources.ErrorOnlyDefinitionFragmentsSupported,
                    reference.Fragment);
            }

            return match.Groups["definitionName"].Captures[0].Value;
        }
    }

    public static class JsonSchemaExtensions
    {
        public static SchemaType SafeGetType(this JsonSchema schema)
        {
            if (schema.Type?.Count > 0) { return schema.Type[0]; }

            if (TryGetTypeFromOneOf(schema.OneOf, out SchemaType typeFromOneOf)) { return typeFromOneOf;  }

            return SchemaType.None;
        }

        // Support a very limited usage of JSON Schema's "oneOf" validation keyword.
        // The reason for this support, and a disclaimer about its limitations, are
        // given in https://github.com/Microsoft/jschema/issues/79.
        private static bool TryGetTypeFromOneOf(IList<JsonSchema> oneOf, out SchemaType result)
        {
            result = SchemaType.None;

            if (oneOf == null || oneOf.Count != 2) { return false; }

            SchemaType firstType = SchemaType.None;
            if (oneOf[0].Type?.Count > 0) { firstType = oneOf[0].Type[0]; }

            SchemaType secondType = SchemaType.None;
            if (oneOf[1].Type?.Count > 0) { secondType = oneOf[1].Type[0]; }

            if ((firstType == SchemaType.Array && secondType == SchemaType.Null) ||
                (firstType == SchemaType.Null  && secondType == SchemaType.Array))
            {
                result = SchemaType.Array;
                return true;
            }

            return false;
        }
    }

    public static class JsonSerializerExtensions
    {
        public static void CaptureError(
            this JsonSerializer serializer,
            JToken jToken,
            ErrorNumber errorNumber,
            params object[] args)
        {
            var exception = new SchemaValidationException(jToken, errorNumber, args);
            serializer.TraceWriter.Trace(TraceLevel.Error, exception.Message, exception);
        }
    }
}
