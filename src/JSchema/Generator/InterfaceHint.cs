// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// an interface in addition to a class.
    /// </summary>
    public class InterfaceHint : CodeGenHint
    {
        /// <summary>
        /// Summary comment for the interface declaration.
        /// </summary>
        public string Description { get; set; }
    }
}
