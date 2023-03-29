// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the code generator to generate a
    /// JSON property with the specified .NET type, instead of deriving the .NET property type
    /// from the schema property.
    /// </summary>
    public class PropertyTypeHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeHint"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The type of the .NET property to generate.
        /// </param>
        public PropertyTypeHint(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            TypeName = typeName;
        }

        /// <summary>
        /// Gets the type of the .NET property to generate.
        /// </summary>
        public string TypeName { get; }
    }
}
