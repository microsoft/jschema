using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Color for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    internal sealed class ColorEqualityComparer : IEqualityComparer<Color>
    {
        internal static readonly ColorEqualityComparer Instance = new ColorEqualityComparer();

        public bool Equals(Color left, Color right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Red != right.Red)
            {
                return false;
            }

            if (left.Green != right.Green)
            {
                return false;
            }

            if (left.Blue != right.Blue)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Color obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Red.GetHashCode();
                result = (result * 31) + obj.Green.GetHashCode();
                result = (result * 31) + obj.Blue.GetHashCode();
            }

            return result;
        }
    }
}