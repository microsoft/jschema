// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Values that specify the C# type to be generated
    /// for each Json Number property.
    /// </summary>
    public enum GenerateJsonNumberOption
    {
        /// <summary>
        /// C# double type
        /// </summary>
        Double,

        /// <summary>
        /// C# float type
        /// </summary>
        Float,

        /// <summary>
        /// C# decimal type
        /// </summary>
        Decimal,

        /// <summary>
        /// Generate type base on the Minimum and Maximum combination
        /// </summary>
        Auto
    }
}
