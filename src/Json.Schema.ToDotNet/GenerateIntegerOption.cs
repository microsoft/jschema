// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Values that specify the C# type to be generated
    /// for each Json Interger property.
    /// </summary>
    public enum GenerateJsonIntegerOption
    {
        /// <summary>
        /// C# int type
        /// </summary>
        Int,

        /// <summary>
        /// C# long type
        /// </summary>
        Long,

        /// <summary>
        /// C# BigInteger type
        /// </summary>
        BigInteger,

        /// <summary>
        /// Generate type base on the Minimum and Maximum combination
        /// </summary>
        Auto
    }
}
