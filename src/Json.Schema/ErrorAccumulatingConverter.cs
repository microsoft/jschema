// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.Json.Schema
{
    public abstract class ErrorAccumulatingConverter : JsonConverter
    {
        protected SchemaValidationErrorAccumulator ErrorAccumulator { get; }

        protected ErrorAccumulatingConverter(SchemaValidationErrorAccumulator errorAccumulator) : base()
        {
            ErrorAccumulator = errorAccumulator;
        }
    }
}
