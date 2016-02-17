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
}
