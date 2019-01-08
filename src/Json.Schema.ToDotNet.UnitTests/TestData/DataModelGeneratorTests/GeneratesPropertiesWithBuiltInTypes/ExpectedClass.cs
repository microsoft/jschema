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

        [DataMember(Name = "stringProperty", IsRequired = false, EmitDefaultValue = false)]
        public string StringProperty { get; set; }
        [DataMember(Name = "numberProperty", IsRequired = false, EmitDefaultValue = false)]
        public double NumberProperty { get; set; }
        [DataMember(Name = "booleanProperty", IsRequired = false, EmitDefaultValue = false)]
        public bool BooleanProperty { get; set; }
        [DataMember(Name = "integerProperty", IsRequired = false, EmitDefaultValue = false)]
        public int IntegerProperty { get; set; }
    }
}