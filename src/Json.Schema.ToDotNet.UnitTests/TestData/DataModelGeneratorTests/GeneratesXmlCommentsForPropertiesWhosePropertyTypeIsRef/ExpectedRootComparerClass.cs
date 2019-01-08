using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace N
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ConsoleWindow for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    internal sealed class ConsoleWindowEqualityComparer : IEqualityComparer<ConsoleWindow>
    {
        internal static readonly ConsoleWindowEqualityComparer Instance = new ConsoleWindowEqualityComparer();

        public bool Equals(ConsoleWindow left, ConsoleWindow right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Color.ValueComparer.Equals(left.ForegroundColor, right.ForegroundColor))
            {
                return false;
            }

            if (!Color.ValueComparer.Equals(left.BackgroundColor, right.BackgroundColor))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(ConsoleWindow obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.ForegroundColor != null)
                {
                    result = (result * 31) + obj.ForegroundColor.ValueGetHashCode();
                }

                if (obj.BackgroundColor != null)
                {
                    result = (result * 31) + obj.BackgroundColor.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}