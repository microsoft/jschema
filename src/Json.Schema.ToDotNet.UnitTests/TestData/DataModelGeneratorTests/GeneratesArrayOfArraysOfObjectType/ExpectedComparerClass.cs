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

            if (!object.ReferenceEquals(left.ArrayOfArrayOfObject, right.ArrayOfArrayOfObject))
            {
                if (left.ArrayOfArrayOfObject == null || right.ArrayOfArrayOfObject == null)
                {
                    return false;
                }

                if (left.ArrayOfArrayOfObject.Count != right.ArrayOfArrayOfObject.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ArrayOfArrayOfObject.Count; ++index_0)
                {
                    if (!object.ReferenceEquals(left.ArrayOfArrayOfObject[index_0], right.ArrayOfArrayOfObject[index_0]))
                    {
                        if (left.ArrayOfArrayOfObject[index_0] == null || right.ArrayOfArrayOfObject[index_0] == null)
                        {
                            return false;
                        }

                        if (left.ArrayOfArrayOfObject[index_0].Count != right.ArrayOfArrayOfObject[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < left.ArrayOfArrayOfObject[index_0].Count; ++index_1)
                        {
                            if (!object.Equals(left.ArrayOfArrayOfObject[index_0][index_1], right.ArrayOfArrayOfObject[index_0][index_1]))
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
                if (obj.ArrayOfArrayOfObject != null)
                {
                    foreach (var value_0 in obj.ArrayOfArrayOfObject)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                if (value_1 != null)
                                {
                                    result = (result * 31) + value_1.GetHashCode();
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