// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Represents the valid values for the "type" keyword in a JSON schema.
    /// </summary>
    public enum SchemaType
    {
        None = 0,

        Array = 1,
        Boolean = 2,
        Integer = 3,
        Number = 4,
        Null = 5,
        Object = 6,
        String = 7,
    }
}