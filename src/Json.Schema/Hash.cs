// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Json.Schema
{
    internal static class Hash
    {
        internal static int Combine(IEnumerable<object> components)
        {
            long hash = 0;
            unchecked
            {
                // Inefficient because it boxes value types. Roslyn has a more elaborate implementation.
                foreach (object component in components.Where(c => !ReferenceEquals(c, null)))
                {
                    // http://stackoverflow.com/questions/2590677/how-do-i-combine-hash-values-in-c0x
                    hash ^= component.GetHashCode() + 0x9e3779b9 + (hash << 6) + (hash >> 2);
                }
            }

            return (int)hash;
        }
    }
}
