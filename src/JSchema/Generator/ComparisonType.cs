// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Values that specify the type of comparison code that needs to be generated
    /// for each property in the implementation of IEquatable&lt;T>.Equals.
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// Do not generate comparison code for this property
        /// </summary>
        None = 0,

        /// <summary>
        /// Compare with a == b.
        /// </summary>
        OperatorEquals,

        /// <summary>
        /// Compare with Object.Equals(a, b).
        /// </summary>
        ObjectEquals,

        /// <summary>
        /// Compare collection elements.
        /// </summary>
        Collection,

        /// <summary>
        /// Compare dictionary entries.
        /// </summary>
        Dictionary
    }
}
