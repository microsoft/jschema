using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// Describes a color with R, G, and B components.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class Color
    {
        public static IEqualityComparer<Color> ValueComparer => ColorEqualityComparer.Instance;

        public bool ValueEquals(Color other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// The value of the R component.
        /// </summary>
        [DataMember(Name = "red", IsRequired = false, EmitDefaultValue = false)]
        public int Red { get; set; }

        /// <summary>
        /// The value of the G component.
        /// </summary>
        [DataMember(Name = "green", IsRequired = false, EmitDefaultValue = false)]
        public int Green { get; set; }

        /// <summary>
        /// The value of the B component.
        /// </summary>
        [DataMember(Name = "blue", IsRequired = false, EmitDefaultValue = false)]
        public int Blue { get; set; }
    }
}