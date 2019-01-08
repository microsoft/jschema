using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class C
    {
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }
    }
}