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
        /// <param name="memberNames">
        /// The names of the enumeration constants to generate, if different from the
        /// string specified in the enum property of theJSON Schema.
        /// </param>
        /// <param name="memberValues">
        /// The numeric values of the enumeration constants, or null if the default
        /// values suffice.
        /// </param>
        /// <param name="zeroValueName">
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
            string[] memberNames,
            int[] memberValues,
            string zeroValueName,
            bool flags)
        {
            TypeName = typeName;
            Description = description;
            MemberNames = memberNames;
            MemberValues = memberValues;
            ZeroValueName = zeroValueName;
            Flags = flags;
        }

        /// <summary>
        /// Gets the name of the enumeration type to generate.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the string to place in the summary comment of the generated type.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the names of the enumeration constants to generate, if different from
        /// the string specified in the enum property of theJSON Schema.
        /// </summary>
        public string[] MemberNames { get; }

        /// <summary>
        /// Gets the numeric values of the enumeration constants, or null of the
        /// default values suffice.
        /// </summary>
        public int[] MemberValues { get; set; }

        /// <summary>
        /// Gets the name of an enumeration constant to represent the 0 value, typically
        /// "Unknown" or "None", if the Enum values from the schema don't include such a
        /// value.
        /// </summary>
        public string ZeroValueName { get; }

        /// <summary>
        /// Gets a value indicating whether the enumeration type is a flags enum.
        /// </summary>
        public bool Flags { get; }

        /// <summary>
        /// Returns a value indicating whether this enum should have a zero value.
        /// </summary>
        public bool HasZeroValue => !string.IsNullOrWhiteSpace(ZeroValueName);
    }
}
