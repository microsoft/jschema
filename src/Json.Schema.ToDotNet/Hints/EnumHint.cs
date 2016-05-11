// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// an enumeration rather than a string-valued .NET property for a JSON property
    /// whose schema specifies an enum keyword.
    /// </summary>
    public class EnumHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumHint"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The name of the enumeration type to generate.
        /// </param>
        /// <param name="description">
        /// The string to place in the summary comment of the generated type.
        /// </param>
        /// <param name="enumValues">
        /// The names of the enumeration constants to generate, if different from the
        /// string specified in the enum property of theJSON Schema.
        /// </param>
        /// <param name="zeroValues">
        /// The name of an enumeration constant to represent the 0 value, typically
        /// "Unknown" or "None", if the enum values from the schema don't include such a
        /// value.
        /// </param>
        /// <param name="flags">
        /// <code>true</code> if the enumeration type is a flags enum; otherwise
        /// <code>false</code>.
        /// </param>
        public EnumHint(
            string typeName,
            string description,
            string[] enumValues,
            string zeroValues,
            bool flags)
        {
            TypeName = typeName;
            Description = description;
            EnumValues = enumValues;
            ZeroValue = zeroValues;
            Flags = flags;
        }

        /// <summary>
        /// Sets the name of the enumeration type to generate.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Sets the string to place in the summary comment of the generated type.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Sets the names of the enumeration constants to generate, if different from
        /// the string specified in the enum property of theJSON Schema.
        /// </summary>
        public string[] EnumValues { get; }

        /// <summary>
        /// Sets the name of an enumeration constant to represent the 0 value, typically
        /// "Unknown" or "None", if the Enum values from the schema don't include such a
        /// value.
        /// </summary>
        public string ZeroValue { get; }

        /// <summary>
        /// Gets a value indicating whether the enumeration type is a flags enum.
        /// </summary>
        public bool Flags { get; }
    }
}
