// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Walks a JSON object.
    /// </summary>
    public abstract class JsonWalker
    {
        public abstract void Walk(JToken jToken);
    }
}
