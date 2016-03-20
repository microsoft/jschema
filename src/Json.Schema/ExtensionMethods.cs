// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Json.Schema
{
    internal static class DictionaryExtensions
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

        internal static bool IsIntegralType(this object obj)
        {
            if (obj == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsFloatingType(this object obj)
        {
            if (obj == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }

    internal static class IEnumerableExtensions
    {
        internal static bool HasSameElementsAs<T>(this IEnumerable<T> left, IEnumerable<T> right)
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

    internal static class StringExtensions
    {
        /// <summary>
        /// Upper-case the first letter of a string to change it from camelCase to
        /// PascalCase.
        /// </summary>
        /// <param name="s">
        /// The string whose first letter is to be upper-cased.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="s"/> in which the first letter has been upper-cased.
        /// </returns>
        internal static string ToPascalCase(this string s)
        {
            return s[0].ToString().ToUpperInvariant() + s.Substring(1);
        }

        /// <summary>
        /// Lower-case the first letter of a string to change it from PascalCase to
        /// camelCase.
        /// </summary>
        /// <param name="s">
        /// The string whose first letter is to be lower-cased.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="s"/> in which the first letter has been lower-cased.
        /// </returns>
        internal static string ToCamelCase(this string s)
        {
            return s[0].ToString().ToLowerInvariant() + s.Substring(1);
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
                throw JSchemaException.Create(
                    Resources.ErrorOnlyDefinitionFragmentsSupported,
                    reference.Fragment);
            }

            return match.Groups["definitionName"].Captures[0].Value;
        }

    }
}
