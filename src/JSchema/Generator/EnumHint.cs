// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// an enumeration rather than a string-valued .NET property for a JSON property
    /// whose schema specifies an enum keyword.
    /// </summary>
    public class EnumHint: CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnumHint"/> class.
        /// </summary>
        public EnumHint()
            : base(Resources.EnumHintName)
        {
        }
    }
}
