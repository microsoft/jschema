// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Serialization;

namespace MountBaker.JSchema
{
    public class JsonSchemaContractResolver : CamelCasePropertyNamesContractResolver
    {
        public override JsonContract ResolveContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            if (objectType == typeof(Uri))
            {
                contract.Converter = UriConverter.Instance;
            }

            return contract;
        }
    }
}
