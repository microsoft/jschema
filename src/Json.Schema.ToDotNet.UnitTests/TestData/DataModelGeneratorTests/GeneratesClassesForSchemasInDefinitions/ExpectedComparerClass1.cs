using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Def1 for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    internal sealed class Def1EqualityComparer : IEqualityComparer<Def1>
    {
        internal static readonly Def1EqualityComparer Instance = new Def1EqualityComparer();

        public bool Equals(Def1 left, Def1 right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Prop1 != right.Prop1)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Def1 obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Prop1 != null)
                {
                    result = (result * 31) + obj.Prop1.GetHashCode();
                }
            }

            return result;
        }
    }
}