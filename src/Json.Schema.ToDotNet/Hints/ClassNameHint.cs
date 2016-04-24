// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// a class with a specified name instead of inferring the name from the
    /// property.
    /// </summary>
    public class ClassNameHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassNameHint"/> class.
        /// </summary>
        /// <param name="className">
        /// The name of the class to generate.
        /// </param>
        public ClassNameHint(string className)
        {
            ClassName = className;
        }

        /// <summary>
        /// Gets the name of the class to generate.
        /// </summary>
        public string ClassName { get; }
    }
}
