// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// A hint that controls certain aspects of code generation.
    /// </summary>
    public abstract class CodeGenHint
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="CodeGenHint"/> class with the
        /// specified name.
        /// </summary>
        /// <param name="name">
        /// A friendly name for this type of hint.
        /// </param>
        protected CodeGenHint(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the friendly name of the hint.
        /// </summary>
        public string Name { get; }
    }
}
