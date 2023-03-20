// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the code generator to generate a
    /// JSON integer property with the specified .NET type, instead of deriving the .NET property type
    /// from the schema property.
    /// </summary>
    public class GenerateJsonIntegerAsHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateJsonIntegerAsHint"/> class.
        /// </summary>
        /// <param name="dotNetPropertyType">
        /// The type of the .NET property to generate.
        /// </param>
        public GenerateJsonIntegerAsHint(string dotNetPropertyType)
        {
            if (dotNetPropertyType == null)
            {
                throw new ArgumentNullException(nameof(dotNetPropertyType));
            }

            DotNetPropertyType = dotNetPropertyType;
        }

        /// <summary>
        /// Gets the type of the .NET property to generate.
        /// </summary>
        public string DotNetPropertyType { get; }
    }
}
