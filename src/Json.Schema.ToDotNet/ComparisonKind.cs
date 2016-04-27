// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Values that specify the type of comparison code that must be generated
    /// for each property in the implementation of the
    /// <see cref="IEquatable{T}.Equals" /> method.
    /// </summary>
    public enum ComparisonKind
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
