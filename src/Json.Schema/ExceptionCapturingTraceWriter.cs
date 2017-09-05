// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Json.Schema
{
    internal class SchemaValidationExceptionCapturingTraceWriter : ITraceWriter
    {
        internal SchemaValidationExceptionCapturingTraceWriter()
        {
            SchemaValidationExceptions = new List<SchemaValidationException>();
        }

        internal List<SchemaValidationException> SchemaValidationExceptions;

        #region ITraceWriter

        public TraceLevel LevelFilter => TraceLevel.Error;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            var schemaValidationException = ex as SchemaValidationException;

            if (schemaValidationException != null)
            {
                SchemaValidationExceptions.Add(schemaValidationException);
            }
        }

        #endregion ITraceWriter
    }
}
