// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace MountBaker.JSchema.ObjectModel
{
    internal static class DictionaryExtensions
    {
        internal static bool HasSameElementsAs<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> other)
        {
            if (dict == null && other == null)
            {
                return true;
            }

            if (dict == null || other == null)
            {
                return false;
            }

            // http://stackoverflow.com/questions/3804367/testing-for-equality-between-dictionaries-in-c-sharp
            return dict.Count == other.Count && !dict.Except(other).Any();
        } 
    }
}
