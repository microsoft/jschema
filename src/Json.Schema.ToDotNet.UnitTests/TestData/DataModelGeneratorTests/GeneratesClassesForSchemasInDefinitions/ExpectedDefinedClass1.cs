using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class Def1
    {
        public static IEqualityComparer<Def1> ValueComparer => Def1EqualityComparer.Instance;

        public bool ValueEquals(Def1 other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = "prop1", IsRequired = false, EmitDefaultValue = false)]
        public string Prop1 { get; set; }
    }
}