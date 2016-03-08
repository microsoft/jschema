// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Contains information about a property inferred from the schema.
    /// </summary>
    public class PropertyInfo
    {
        /// <summary>
        /// Gets or sets a value that specifies the type of comparison code that needs
        /// to be generated for the property in the implementation of
        /// IEquatable&lt;T>.Equals.
        /// </summary>
        public ComparisonType ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the type of code that needs to be
        /// generated to compute the for the property in the implementation of
        /// <see cref="Object.GetHashCode" />.
        /// </summary>
        public HashType HashType { get; set; }
    }
}
