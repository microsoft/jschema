// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Json.Schema
{
    public class JsonSchemaContractResolver : CamelCasePropertyNamesContractResolver
    {
        private static readonly Dictionary<Type, JsonConverter> s_typeToConverterDictionary =
            new Dictionary<Type, JsonConverter>
            {
                [typeof(UriOrFragment)] = UriOrFragmentConverter.Instance,
                [typeof(AdditionalItems)] = AdditionalItemsConverter.Instance,
                [typeof(AdditionalProperties)] = AdditionalPropertiesConverter.Instance,
                [typeof(Items)] = ItemsConverter.Instance,
                [typeof(Dependency)] = DependencyConverter.Instance
            };

        public override JsonContract ResolveContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            JsonConverter converter;
            if (s_typeToConverterDictionary.TryGetValue(objectType, out converter))
            {
                contract.Converter = converter;
            }

            return contract;
        }
    }
}
