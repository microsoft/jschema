// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class ObjectExtensions
    {
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
