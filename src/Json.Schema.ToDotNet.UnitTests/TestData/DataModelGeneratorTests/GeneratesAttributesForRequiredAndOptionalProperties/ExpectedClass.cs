using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class C
    {
        [DataMember(Name = "requiredProp1", IsRequired = true)]
        public string RequiredProp1 { get; set; }
        [DataMember(Name = "optionalProp", IsRequired = false, EmitDefaultValue = false)]
        public string OptionalProp { get; set; }
        [DataMember(Name = "requiredProp2", IsRequired = true)]
        public string RequiredProp2 { get; set; }
    }
}