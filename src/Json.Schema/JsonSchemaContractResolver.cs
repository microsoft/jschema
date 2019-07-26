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
        private readonly SchemaValidationErrorAccumulator _errorAccumulator;

        public JsonSchemaContractResolver(SchemaValidationErrorAccumulator errorAccumulator)
        {
            _errorAccumulator = errorAccumulator;
        }

        public override JsonContract ResolveContract(Type objectType)
        {
            Dictionary<Type, JsonConverter> typeToConverterDictionary =
                new Dictionary<Type, JsonConverter>
                {
                    [typeof(UriOrFragment)] = new UriOrFragmentConverter(_errorAccumulator),
                    [typeof(AdditionalItems)] = new AdditionalItemsConverter(_errorAccumulator),
                    [typeof(AdditionalProperties)] = new AdditionalPropertiesConverter(_errorAccumulator),
                    [typeof(Items)] = new ItemsConverter(_errorAccumulator),
                    [typeof(Dependency)] = new DependencyConverter(_errorAccumulator),
                    [typeof(SchemaType[])] = new SchemaTypeConverter(_errorAccumulator)
                };

            var contract = base.CreateContract(objectType);

            JsonConverter converter;
            if (typeToConverterDictionary.TryGetValue(objectType, out converter))
            {
                contract.Converter = converter;
            }

            return contract;
        }
    }
}
