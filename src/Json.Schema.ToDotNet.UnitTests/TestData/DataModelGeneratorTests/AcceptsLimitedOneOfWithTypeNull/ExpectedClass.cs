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
        /// <summary>
        /// A property that can either be an array or null.
        /// </summary>
        [DataMember(Name = "arrayOrNullProperty", IsRequired = false, EmitDefaultValue = false)]
        public IList<int> ArrayOrNullProperty { get; set; }
    }
}