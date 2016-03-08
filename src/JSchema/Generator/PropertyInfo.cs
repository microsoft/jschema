// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Contains information about a property inferred from the schema.
    /// </summary>
    public class PropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfo"/> class.
        /// </summary>
        /// <param name="comparisonKind">
        /// The kind of comparison code required by the property.
        /// </param>
        /// <param name="hashKind">
        /// The kind of hash value computation code required by the property.
        /// </param>
        /// <param name="initializationKind">
        /// The kind of initialization code required by the property.
        /// </param>
        /// <param name="type">
        /// The type of the property.
        /// </param>
        public PropertyInfo(
            ComparisonKind comparisonKind,
            HashKind hashKind,
            InitializationKind initializationKind,
            TypeSyntax type)
        {
            ComparisonKind = comparisonKind;
            HashKind = hashKind;
            InitializationKind = initializationKind;
            Type = type;
        }

        /// <summary>
        /// Gets a value that specifies the kind of comparison code that must be
        /// generated for the property in the implementation of the
        /// <code>IEquatable&lt;T>.Equals</code> method.
        /// </summary>
        public ComparisonKind ComparisonKind { get; }

        /// <summary>
        /// Gets a value that specifies the kind of code that must be generated to
        /// compute the hash for the property in the implementation of the
        /// <see cref="Object.GetHashCode" /> method.
        /// </summary>
        public HashKind HashKind { get; }

        /// <summary>
        /// Gets a value that specifies the kind of initialization code that must be
        /// generated for the property in the implementation of the <code>Init</code>
        /// method.
        /// </summary>
        public InitializationKind InitializationKind { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public TypeSyntax Type { get; }
    }
}
