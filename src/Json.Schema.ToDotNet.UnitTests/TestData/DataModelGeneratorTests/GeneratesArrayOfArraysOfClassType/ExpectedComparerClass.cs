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

            if (!object.ReferenceEquals(left.ArrayOfArrayOfD, right.ArrayOfArrayOfD))
            {
                if (left.ArrayOfArrayOfD == null || right.ArrayOfArrayOfD == null)
                {
                    return false;
                }

                if (left.ArrayOfArrayOfD.Count != right.ArrayOfArrayOfD.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfArrayOfD.Count; ++index_0)
                {
                    if (!object.ReferenceEquals(left.ArrayOfArrayOfD[index_0], right.ArrayOfArrayOfD[index_0]))
                    {
                        if (left.ArrayOfArrayOfD[index_0] == null || right.ArrayOfArrayOfD[index_0] == null)
                        {
                            return false;
                        }

                        if (left.ArrayOfArrayOfD[index_0].Count != right.ArrayOfArrayOfD[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < left.ArrayOfArrayOfD[index_0].Count; ++index_1)
                        {
                            if (!D.ValueComparer.Equals(left.ArrayOfArrayOfD[index_0][index_1], right.ArrayOfArrayOfD[index_0][index_1]))
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
                if (obj.ArrayOfArrayOfD != null)
                {
                    foreach (var value_0 in obj.ArrayOfArrayOfD)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                if (value_1 != null)
                                {
                                    result = (result * 31) + value_1.ValueGetHashCode();
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}