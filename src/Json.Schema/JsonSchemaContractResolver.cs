// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Json.Schema
{
    public class JsonSchemaContractResolver : CamelCasePropertyNamesContractResolver
    {
        public override JsonContract ResolveContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            if (objectType == typeof(UriOrFragment))
            {
                contract.Converter = UriOrFragmentConverter.Instance;
            }
            else if (objectType == typeof(AdditionalProperties))
            {
                contract.Converter = AdditionalPropertiesConverter.Instance;
            }

            return contract;
        }
    }
}
