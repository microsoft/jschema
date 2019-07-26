// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Converts a property of type <see cref="UriOrFragment"/> to or from a string
    /// during serialization or deserialization.
    /// </summary>
    internal class UriOrFragmentConverter : JsonConverter
    {
        public static readonly UriOrFragmentConverter Instance = new UriOrFragmentConverter();

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