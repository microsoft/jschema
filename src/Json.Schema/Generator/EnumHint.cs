// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.Generator
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// an enumeration rather than a string-valued .NET property for a JSON property
    /// whose schema specifies an enum keyword.
    /// </summary>
    public class EnumHint : CodeGenHint
    {
        /// <summary>
        /// Gets or sets the name of the enumeration type to generate.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the string to place in the summary comment of the generated
        /// type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the names of the enumeration constants to generate, if different
        /// from the string specified in the enum property of theJSON Schema.
        /// </summary>
        public string[] Enum { get; set; }

        /// <summary>
        /// Gets or sets the name of an enumeration constant to represent the 0 value
        /// typically "Unknown" or "None", if the Enum values from the schema don't
        /// include such a value.
        /// </summary>
        public string ZeroValue { get; set; }
    }
}
