// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    public static class JTokenTypeExtensions
    {
        public static SchemaType ToSchemaType(this JTokenType jTokenType)
        {
            switch (jTokenType)
            {
                case JTokenType.Array:
                    return SchemaType.Array;

                case JTokenType.Boolean:
                    return SchemaType.Boolean;

                case JTokenType.Date:
                    return SchemaType.String;

                case JTokenType.Float:
                    return SchemaType.Number;

                case JTokenType.Integer:
                    return SchemaType.Integer;

                case JTokenType.Null:
                    return SchemaType.Null;

                case JTokenType.Object:
                    return SchemaType.Object;

                case JTokenType.String:
                    return SchemaType.String;

                case JTokenType.Uri:
                    return SchemaType.String;

                case JTokenType.TimeSpan:
                    return SchemaType.String;

                default:
                    return SchemaType.None;                 
            }
        }
    }
}
