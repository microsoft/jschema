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

            if (left.StringProperty != right.StringProperty)
            {
                return false;
            }

            if (left.NumberProperty != right.NumberProperty)
            {
                return false;
            }

            if (left.BooleanProperty != right.BooleanProperty)
            {
                return false;
            }

            if (left.IntegerProperty != right.IntegerProperty)
            {
                return false;
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
                if (obj.StringProperty != null)
                {
                    result = (result * 31) + obj.StringProperty.GetHashCode();
                }

                result = (result * 31) + obj.NumberProperty.GetHashCode();
                result = (result * 31) + obj.BooleanProperty.GetHashCode();
                result = (result * 31) + obj.IntegerProperty.GetHashCode();
            }

            return result;
        }
    }
}