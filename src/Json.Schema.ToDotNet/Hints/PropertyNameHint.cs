// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the code generator to generate a
    /// property with the specified name, instead of deriving the .NET property name
    /// from the schema property name.
    /// </summary>
    public class PropertyNameHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNameHint"/> class.
        /// </summary>
        /// <param name="dotNetPropertyName">
        /// The name of the .NET property to generate.
        /// </param>
        public PropertyNameHint(string dotNetPropertyName)
        {
            if (dotNetPropertyName == null)
            {
                throw new ArgumentNullException(nameof(dotNetPropertyName));
            }

            DotNetPropertyName = dotNetPropertyName;
        }

        /// <summary>
        /// Gets the name of the .NET property to generate.
        /// </summary>
        public string DotNetPropertyName { get; }
    }
}
