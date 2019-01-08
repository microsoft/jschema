using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type C for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    internal sealed class CEqualityComparer : IEqualityComparer<C>
    {
        internal static readonly CEqualityComparer Instance = new CEqualityComparer();

        public bool Equals(C left, C right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.UriFormattedStrings, right.UriFormattedStrings))
            {
                if (left.UriFormattedStrings == null || right.UriFormattedStrings == null)
                {
                    return false;
                }

                if (left.UriFormattedStrings.Count != right.UriFormattedStrings.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.UriFormattedStrings.Count; ++index_0)
                {
                    if (left.UriFormattedStrings[index_0] != right.UriFormattedStrings[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(C obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.UriFormattedStrings != null)
                {
                    foreach (var value_0 in obj.UriFormattedStrings)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}