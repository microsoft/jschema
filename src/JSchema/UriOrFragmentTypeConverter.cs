// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.JSchema
{
    /// <summary>
    /// Converts a dictionary key of type <see cref="UriOrFragment"/> to or from a string
    /// during serialization or deserialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Json.NET uses a <see cref="TypeConverter"/> when serializing or deserializing the
    /// keys of a .NET <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>.
    /// But it uses a <see cref="Newtonsoft.Json.JsonConverter"/> when serializing or
    /// deserializing the properties of a .NET object.
    /// </para>
    /// <para>
    /// The <see cref="UriOrFragment"/> class is used both as the type of a dictionary
    /// key (in <see cref="Generator.HintDictionary"/>), and as the type of a .NET
    /// property (in <see cref="JsonSchema"/>). Therefore we must supply both this class
    /// and the class <see cref="UriOrFragmentJsonConverter"/>
    /// </para>
    /// </remarks>
    public class UriOrFragmentTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
            string s = value as string;
            if (s != null)
            {
                return new UriOrFragment(s);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return (value as UriOrFragment).ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}