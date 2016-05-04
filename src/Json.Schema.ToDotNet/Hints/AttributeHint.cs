// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to add
    /// an attribute to a property.
    /// </summary>
    public class AttributeHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeHint"/> class.
        /// </summary>
        /// <param name="attributeTypeName">
        /// The fully qualified type name of the attribute to generate, without
        /// the "Attribute" suffix.
        /// </param>
        public AttributeHint(string attributeTypeName)
        {
            AttributeTypeName = attributeTypeName;
        }

        /// <summary>
        /// Gets the fully qualified type name of the attribute to generate, without
        /// the "Attribute" suffix.
        /// </summary>
        public string AttributeTypeName { get; }
    }
}
