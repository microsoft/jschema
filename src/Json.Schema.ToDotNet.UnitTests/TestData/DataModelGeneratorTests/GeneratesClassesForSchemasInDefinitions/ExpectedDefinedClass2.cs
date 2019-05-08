using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class Def2
    {
        public static IEqualityComparer<Def2> ValueComparer => Def2EqualityComparer.Instance;

        public bool ValueEquals(Def2 other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = "prop2", IsRequired = false, EmitDefaultValue = false)]
        public int Prop2 { get; set; }
    }
}