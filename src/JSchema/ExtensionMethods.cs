// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.JSchema
{
    internal static class DictionaryExtensions
    {
        internal static bool HasSameElementsAs<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> other)
        {
            if (dict == null && other == null)
            {
                return true;
            }

            if (dict == null || other == null)
            {
                return false;
            }

            // http://stackoverflow.com/questions/3804367/testing-for-equality-between-dictionaries-in-c-sharp
            return dict.Count == other.Count && !dict.Except(other).Any();
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
}
