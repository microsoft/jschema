// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Json.Schema
{
    internal class ErrorCapturingTraceWriter : ITraceWriter
    {
        internal ErrorCapturingTraceWriter()
        {
            Errors = new List<Error>();
        }

        internal List<Error> Errors;

        #region ITraceWriter

        public TraceLevel LevelFilter => TraceLevel.Error;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            var invalidSchemaException = ex as InvalidSchemaException;
            if (invalidSchemaException != null && invalidSchemaException.Errors.Any())
            {
                Errors.AddRange(invalidSchemaException.Errors);
            }
        }

        #endregion ITraceWriter
    }
}
