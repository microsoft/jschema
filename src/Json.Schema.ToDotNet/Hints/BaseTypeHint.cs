// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the code generator to declare one or
    /// more base types for the specified type.
    /// </summary>
    public class BaseTypeHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeHint"/> class.
        /// </summary>
        /// <param name="baseTypeNames">
        /// The names of the base types.
        /// </param>
        public BaseTypeHint(IEnumerable<string> baseTypeNames)
        {
            BaseTypeNames = baseTypeNames.ToList();
        }

        /// <summary>
        /// Gets the names of the base types.
        /// </summary>
        public IList<string> BaseTypeNames { get; }
    }
}
