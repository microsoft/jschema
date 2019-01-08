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
    public partial class C : ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISNode" />.
        /// </summary>
        public SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

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
        [DefaultValue("Don't panic.")]
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
        /// An enumerated property with a default value.
        /// </summary>
        [DataMember(Name = "enumeratedPropertyWithDefault", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(Color.Green)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Color EnumeratedPropertyWithDefault { get; set; }

        /// <summary>
        /// An array property.
        /// </summary>
        [DataMember(Name = "arrayProp", IsRequired = false, EmitDefaultValue = false)]
        public IList<double> ArrayProp { get; set; }

        /// <summary>
        /// A Uri property.
        /// </summary>
        [DataMember(Name = "uriProp", IsRequired = false, EmitDefaultValue = false)]
        public Uri UriProp { get; set; }

        /// <summary>
        /// A DateTime property.
        /// </summary>
        [DataMember(Name = "dateTimeProp", IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTimeProp { get; set; }
        [DataMember(Name = "referencedTypeProp", IsRequired = false, EmitDefaultValue = false)]
        public D ReferencedTypeProp { get; set; }

        /// <summary>
        /// An array of a cloneable type.
        /// </summary>
        [DataMember(Name = "arrayOfRefProp", IsRequired = false, EmitDefaultValue = false)]
        public IList<D> ArrayOfRefProp { get; set; }

        /// <summary>
        /// An array of arrays.
        /// </summary>
        [DataMember(Name = "arrayOfArrayProp", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<D>> ArrayOfArrayProp { get; set; }

        /// <summary>
        /// A dictionary property.
        /// </summary>
        [DataMember(Name = "dictionaryProp", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> DictionaryProp { get; set; }

        /// <summary>
        /// A dictionary property whose values are defined by a primitive additionalProperties schema.
        /// </summary>
        [DataMember(Name = "dictionaryWithPrimitiveSchemaProp", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, double> DictionaryWithPrimitiveSchemaProp { get; set; }

        /// <summary>
        /// A dictionary property whose values are defined by an object-valued additionalProperties schema.
        /// </summary>
        [DataMember(Name = "dictionaryWithObjectSchemaProp", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, D> DictionaryWithObjectSchemaProp { get; set; }

        /// <summary>
        /// A dictionary property whose values are defined by an array-of-object-valued additionalProperties schema.
        /// </summary>
        [DataMember(Name = "dictionaryWithObjectArraySchemaProp", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<D>> DictionaryWithObjectArraySchemaProp { get; set; }

        /// <summary>
        /// A dictionary property whose keys are Uris.
        /// </summary>
        [DataMember(Name = "dictionaryWithUriKeyProp", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<Uri, D> DictionaryWithUriKeyProp { get; set; }

        /// <summary>
        /// A dictionary property whose value type is hinted.
        /// </summary>
        [DataMember(Name = "dictionaryWithHintedValueProp", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, V> DictionaryWithHintedValueProp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="C" /> class.
        /// </summary>
        public C()
        {
            IntegerPropertyWithDefault = 42;
            NumberPropertyWithDefault = 42.1;
            StringPropertyWithDefault = "Don't panic.";
            BooleanPropertyWithTrueDefault = true;
            BooleanPropertyWithFalseDefault = false;
            EnumeratedPropertyWithDefault = Color.Green;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C" /> class from the supplied values.
        /// </summary>
        /// <param name="integerProperty">
        /// An initialization value for the <see cref="P:IntegerProperty" /> property.
        /// </param>
        /// <param name="integerPropertyWithDefault">
        /// An initialization value for the <see cref="P:IntegerPropertyWithDefault" /> property.
        /// </param>
        /// <param name="numberProperty">
        /// An initialization value for the <see cref="P:NumberProperty" /> property.
        /// </param>
        /// <param name="numberPropertyWithDefault">
        /// An initialization value for the <see cref="P:NumberPropertyWithDefault" /> property.
        /// </param>
        /// <param name="stringProperty">
        /// An initialization value for the <see cref="P:StringProperty" /> property.
        /// </param>
        /// <param name="stringPropertyWithDefault">
        /// An initialization value for the <see cref="P:StringPropertyWithDefault" /> property.
        /// </param>
        /// <param name="booleanProperty">
        /// An initialization value for the <see cref="P:BooleanProperty" /> property.
        /// </param>
        /// <param name="booleanPropertyWithTrueDefault">
        /// An initialization value for the <see cref="P:BooleanPropertyWithTrueDefault" /> property.
        /// </param>
        /// <param name="booleanPropertyWithFalseDefault">
        /// An initialization value for the <see cref="P:BooleanPropertyWithFalseDefault" /> property.
        /// </param>
        /// <param name="enumeratedPropertyWithDefault">
        /// An initialization value for the <see cref="P:EnumeratedPropertyWithDefault" /> property.
        /// </param>
        /// <param name="arrayProp">
        /// An initialization value for the <see cref="P:ArrayProp" /> property.
        /// </param>
        /// <param name="uriProp">
        /// An initialization value for the <see cref="P:UriProp" /> property.
        /// </param>
        /// <param name="dateTimeProp">
        /// An initialization value for the <see cref="P:DateTimeProp" /> property.
        /// </param>
        /// <param name="referencedTypeProp">
        /// An initialization value for the <see cref="P:ReferencedTypeProp" /> property.
        /// </param>
        /// <param name="arrayOfRefProp">
        /// An initialization value for the <see cref="P:ArrayOfRefProp" /> property.
        /// </param>
        /// <param name="arrayOfArrayProp">
        /// An initialization value for the <see cref="P:ArrayOfArrayProp" /> property.
        /// </param>
        /// <param name="dictionaryProp">
        /// An initialization value for the <see cref="P:DictionaryProp" /> property.
        /// </param>
        /// <param name="dictionaryWithPrimitiveSchemaProp">
        /// An initialization value for the <see cref="P:DictionaryWithPrimitiveSchemaProp" /> property.
        /// </param>
        /// <param name="dictionaryWithObjectSchemaProp">
        /// An initialization value for the <see cref="P:DictionaryWithObjectSchemaProp" /> property.
        /// </param>
        /// <param name="dictionaryWithObjectArraySchemaProp">
        /// An initialization value for the <see cref="P:DictionaryWithObjectArraySchemaProp" /> property.
        /// </param>
        /// <param name="dictionaryWithUriKeyProp">
        /// An initialization value for the <see cref="P:DictionaryWithUriKeyProp" /> property.
        /// </param>
        /// <param name="dictionaryWithHintedValueProp">
        /// An initialization value for the <see cref="P:DictionaryWithHintedValueProp" /> property.
        /// </param>
        public C(int integerProperty, int integerPropertyWithDefault, double numberProperty, double numberPropertyWithDefault, string stringProperty, string stringPropertyWithDefault, bool booleanProperty, bool booleanPropertyWithTrueDefault, bool booleanPropertyWithFalseDefault, Color enumeratedPropertyWithDefault, IEnumerable<double> arrayProp, Uri uriProp, DateTime dateTimeProp, D referencedTypeProp, IEnumerable<D> arrayOfRefProp, IEnumerable<IEnumerable<D>> arrayOfArrayProp, IDictionary<string, string> dictionaryProp, IDictionary<string, double> dictionaryWithPrimitiveSchemaProp, IDictionary<string, D> dictionaryWithObjectSchemaProp, IDictionary<string, IList<D>> dictionaryWithObjectArraySchemaProp, IDictionary<Uri, D> dictionaryWithUriKeyProp, IDictionary<string, V> dictionaryWithHintedValueProp)
        {
            Init(integerProperty, integerPropertyWithDefault, numberProperty, numberPropertyWithDefault, stringProperty, stringPropertyWithDefault, booleanProperty, booleanPropertyWithTrueDefault, booleanPropertyWithFalseDefault, enumeratedPropertyWithDefault, arrayProp, uriProp, dateTimeProp, referencedTypeProp, arrayOfRefProp, arrayOfArrayProp, dictionaryProp, dictionaryWithPrimitiveSchemaProp, dictionaryWithObjectSchemaProp, dictionaryWithObjectArraySchemaProp, dictionaryWithUriKeyProp, dictionaryWithHintedValueProp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.IntegerProperty, other.IntegerPropertyWithDefault, other.NumberProperty, other.NumberPropertyWithDefault, other.StringProperty, other.StringPropertyWithDefault, other.BooleanProperty, other.BooleanPropertyWithTrueDefault, other.BooleanPropertyWithFalseDefault, other.EnumeratedPropertyWithDefault, other.ArrayProp, other.UriProp, other.DateTimeProp, other.ReferencedTypeProp, other.ArrayOfRefProp, other.ArrayOfArrayProp, other.DictionaryProp, other.DictionaryWithPrimitiveSchemaProp, other.DictionaryWithObjectSchemaProp, other.DictionaryWithObjectArraySchemaProp, other.DictionaryWithUriKeyProp, other.DictionaryWithHintedValueProp);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        private void Init(int integerProperty, int integerPropertyWithDefault, double numberProperty, double numberPropertyWithDefault, string stringProperty, string stringPropertyWithDefault, bool booleanProperty, bool booleanPropertyWithTrueDefault, bool booleanPropertyWithFalseDefault, Color enumeratedPropertyWithDefault, IEnumerable<double> arrayProp, Uri uriProp, DateTime dateTimeProp, D referencedTypeProp, IEnumerable<D> arrayOfRefProp, IEnumerable<IEnumerable<D>> arrayOfArrayProp, IDictionary<string, string> dictionaryProp, IDictionary<string, double> dictionaryWithPrimitiveSchemaProp, IDictionary<string, D> dictionaryWithObjectSchemaProp, IDictionary<string, IList<D>> dictionaryWithObjectArraySchemaProp, IDictionary<Uri, D> dictionaryWithUriKeyProp, IDictionary<string, V> dictionaryWithHintedValueProp)
        {
            IntegerProperty = integerProperty;
            IntegerPropertyWithDefault = integerPropertyWithDefault;
            NumberProperty = numberProperty;
            NumberPropertyWithDefault = numberPropertyWithDefault;
            StringProperty = stringProperty;
            StringPropertyWithDefault = stringPropertyWithDefault;
            BooleanProperty = booleanProperty;
            BooleanPropertyWithTrueDefault = booleanPropertyWithTrueDefault;
            BooleanPropertyWithFalseDefault = booleanPropertyWithFalseDefault;
            EnumeratedPropertyWithDefault = enumeratedPropertyWithDefault;
            if (arrayProp != null)
            {
                var destination_0 = new List<double>();
                foreach (var value_0 in arrayProp)
                {
                    destination_0.Add(value_0);
                }

                ArrayProp = destination_0;
            }

            if (uriProp != null)
            {
                UriProp = new Uri(uriProp.OriginalString, uriProp.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            DateTimeProp = dateTimeProp;
            if (referencedTypeProp != null)
            {
                ReferencedTypeProp = new D(referencedTypeProp);
            }

            if (arrayOfRefProp != null)
            {
                var destination_1 = new List<D>();
                foreach (var value_1 in arrayOfRefProp)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new D(value_1));
                    }
                }

                ArrayOfRefProp = destination_1;
            }

            if (arrayOfArrayProp != null)
            {
                var destination_2 = new List<IList<D>>();
                foreach (var value_2 in arrayOfArrayProp)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        var destination_3 = new List<D>();
                        foreach (var value_3 in value_2)
                        {
                            if (value_3 == null)
                            {
                                destination_3.Add(null);
                            }
                            else
                            {
                                destination_3.Add(new D(value_3));
                            }
                        }

                        destination_2.Add(destination_3);
                    }
                }

                ArrayOfArrayProp = destination_2;
            }

            if (dictionaryProp != null)
            {
                DictionaryProp = new Dictionary<string, string>(dictionaryProp);
            }

            if (dictionaryWithPrimitiveSchemaProp != null)
            {
                DictionaryWithPrimitiveSchemaProp = new Dictionary<string, double>(dictionaryWithPrimitiveSchemaProp);
            }

            if (dictionaryWithObjectSchemaProp != null)
            {
                DictionaryWithObjectSchemaProp = new Dictionary<string, D>();
                foreach (var value_4 in dictionaryWithObjectSchemaProp)
                {
                    DictionaryWithObjectSchemaProp.Add(value_4.Key, new D(value_4.Value));
                }
            }

            if (dictionaryWithObjectArraySchemaProp != null)
            {
                DictionaryWithObjectArraySchemaProp = new Dictionary<string, IList<D>>();
                foreach (var value_5 in dictionaryWithObjectArraySchemaProp)
                {
                    var destination_4 = new List<D>();
                    foreach (var value_6 in value_5.Value)
                    {
                        if (value_6 == null)
                        {
                            destination_4.Add(null);
                        }
                        else
                        {
                            destination_4.Add(new D(value_6));
                        }
                    }

                    DictionaryWithObjectArraySchemaProp.Add(value_5.Key, destination_4);
                }
            }

            if (dictionaryWithUriKeyProp != null)
            {
                DictionaryWithUriKeyProp = new Dictionary<Uri, D>();
                foreach (var value_7 in dictionaryWithUriKeyProp)
                {
                    DictionaryWithUriKeyProp.Add(value_7.Key, new D(value_7.Value));
                }
            }

            if (dictionaryWithHintedValueProp != null)
            {
                DictionaryWithHintedValueProp = new Dictionary<string, V>(dictionaryWithHintedValueProp);
            }
        }
    }
}