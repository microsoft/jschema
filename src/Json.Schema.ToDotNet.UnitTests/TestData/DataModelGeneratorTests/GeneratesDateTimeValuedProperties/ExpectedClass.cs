using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class C
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        [DataMember(Name = "startTime", IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartTime { get; set; }
    }
}