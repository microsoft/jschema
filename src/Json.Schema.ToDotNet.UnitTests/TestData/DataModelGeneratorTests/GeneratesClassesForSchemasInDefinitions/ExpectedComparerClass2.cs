using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Def2 for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    internal sealed class Def2EqualityComparer : IEqualityComparer<Def2>
    {
        internal static readonly Def2EqualityComparer Instance = new Def2EqualityComparer();

        public bool Equals(Def2 left, Def2 right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Prop2 != right.Prop2)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Def2 obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Prop2.GetHashCode();
            }

            return result;
        }
    }
}