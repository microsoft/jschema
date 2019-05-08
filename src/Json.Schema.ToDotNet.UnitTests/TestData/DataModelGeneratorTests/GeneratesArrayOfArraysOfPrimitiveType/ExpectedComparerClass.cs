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

            if (!object.ReferenceEquals(left.ArrayOfArrayOfInt, right.ArrayOfArrayOfInt))
            {
                if (left.ArrayOfArrayOfInt == null || right.ArrayOfArrayOfInt == null)
                {
                    return false;
                }

                if (left.ArrayOfArrayOfInt.Count != right.ArrayOfArrayOfInt.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfArrayOfInt.Count; ++index_0)
                {
                    if (!object.ReferenceEquals(left.ArrayOfArrayOfInt[index_0], right.ArrayOfArrayOfInt[index_0]))
                    {
                        if (left.ArrayOfArrayOfInt[index_0] == null || right.ArrayOfArrayOfInt[index_0] == null)
                        {
                            return false;
                        }

                        if (left.ArrayOfArrayOfInt[index_0].Count != right.ArrayOfArrayOfInt[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < left.ArrayOfArrayOfInt[index_0].Count; ++index_1)
                        {
                            if (left.ArrayOfArrayOfInt[index_0][index_1] != right.ArrayOfArrayOfInt[index_0][index_1])
                            {
                                return false;
                            }
                        }
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
                if (obj.ArrayOfArrayOfInt != null)
                {
                    foreach (var value_0 in obj.ArrayOfArrayOfInt)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                result = (result * 31) + value_1.GetHashCode();
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}