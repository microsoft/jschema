// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.JSchema
{
    /// <summary>
    /// Converts a property of type <see cref="UriOrFragment"/> to or from a string
    /// during serialization or deserialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Json.NET uses a <see cref="JsonConverter"/> when serializing or deserializing the
    /// properties of a .NET object. But when serializing or deserializing the keys of a
    /// .NET <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>, it uses a
    /// <see cref="System.ComponentModel.TypeConverter"/>.
    /// </para>
    /// <para>
    /// The <see cref="UriOrFragment"/> class is used both as the type of a .NET property
    /// (in <see cref="JsonSchema"/>) and as the type of a dictionary key (in
    /// <see cref="Generator.HintDictionary"/>). Therefore we must supply both this class
    /// and the class <see cref="UriOrFragmentTypeConverter"/>
    /// </para>
    /// </remarks>
    internal class UriOrFragmentJsonConverter : JsonConverter
    {
        public static readonly UriOrFragmentJsonConverter Instance = new UriOrFragmentJsonConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UriOrFragment);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string uriString = (string)reader.Value;

            return new UriOrFragment(uriString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue('"' + ((UriOrFragment)value).ToString() + '"');
        }
    }
}