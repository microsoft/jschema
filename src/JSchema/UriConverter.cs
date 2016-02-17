// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.JSchema
{
    internal class UriConverter : JsonConverter
    {
        public static readonly UriConverter Instance = new UriConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Uri);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string uriString = (string)reader.Value;

            return new Uri(uriString, UriKind.RelativeOrAbsolute);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue('"' + ((Uri)value).AbsoluteUri + '"');
        }
    }
}