// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Supported C# type to be generated
    /// for each Json property.
    /// </summary>
    public enum SupportedPropertyTypeHint
    {
        Auto,
        Int,
        Long,
        BigInteger,
        Double,
        Float,
        Decimal,
        DateTime,
        Uri,
        Guid,
        Boolean,
        String
    }
}
