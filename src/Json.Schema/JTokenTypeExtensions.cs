// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    public static class JTokenTypeExtensions
    {
        public static string ToJsonSchemaName(this JTokenType jTokenType)
        {
            return jTokenType == JTokenType.Float
                ? "number"
                : jTokenType.ToString().ToLowerInvariant();

        }
    }
}
