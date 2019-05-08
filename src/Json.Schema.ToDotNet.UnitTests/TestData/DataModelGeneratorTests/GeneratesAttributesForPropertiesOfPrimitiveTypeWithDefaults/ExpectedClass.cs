using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class C
    {
        /// <summary>
        /// An integer property.
        /// </summary>
        [DataMember(Name = "integerProperty", IsRequired = false, EmitDefaultValue = false)]
        public int IntegerProperty { get; set; }

        /// <summary>
        /// An integer property with a default value.
        /// </summary>
        [DataMember(Name = "integerPropertyWithDefault", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(42)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int IntegerPropertyWithDefault { get; set; }

        /// <summary>
        /// A number property.
        /// </summary>
        [DataMember(Name = "numberProperty", IsRequired = false, EmitDefaultValue = false)]
        public double NumberProperty { get; set; }

        /// <summary>
        /// A number property with a default value.
        /// </summary>
        [DataMember(Name = "numberPropertyWithDefault", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(42.1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double NumberPropertyWithDefault { get; set; }

        /// <summary>
        /// A string property.
        /// </summary>
        [DataMember(Name = "stringProperty", IsRequired = false, EmitDefaultValue = false)]
        public string StringProperty { get; set; }

        /// <summary>
        /// A string property with a default value.
        /// </summary>
        [DataMember(Name = "stringPropertyWithDefault", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("Thanks for all the fish.")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StringPropertyWithDefault { get; set; }

        /// <summary>
        /// A Boolean property.
        /// </summary>
        [DataMember(Name = "booleanProperty", IsRequired = false, EmitDefaultValue = false)]
        public bool BooleanProperty { get; set; }

        /// <summary>
        /// A Boolean property with a true default value.
        /// </summary>
        [DataMember(Name = "booleanPropertyWithTrueDefault", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool BooleanPropertyWithTrueDefault { get; set; }

        /// <summary>
        /// A Boolean property with a false default value.
        /// </summary>
        [DataMember(Name = "booleanPropertyWithFalseDefault", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool BooleanPropertyWithFalseDefault { get; set; }

        /// <summary>
        /// A non-primitive property with a default value: DefaultValue attribute will -not- be emitted.
        /// </summary>
        [DataMember(Name = "nonPrimitivePropertyWithDefault", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<int> NonPrimitivePropertyWithDefault { get; set; }
    }
}