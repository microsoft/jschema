// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema
{
    internal static class Hash
    {
        internal static int Combine(params object[] components)
        {
            long hash = 0;
            unchecked
            {
                // Inefficient because it boxes value types. Roslyn has a more elaborate implementation.
                foreach (object component in components)
                {
                    // http://stackoverflow.com/questions/2590677/how-do-i-combine-hash-values-in-c0x
                    hash ^= component.GetHashCode() + 0x9e3779b9 + (hash << 6) + (hash >> 2);
                }
            }

            return (int)hash;
        }
    }
}
