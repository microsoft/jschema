// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    public class SchemaValidationErrorAccumulator
    {
        [ThreadStatic]
        private static SchemaValidationErrorAccumulator s_instance;

        public static SchemaValidationErrorAccumulator Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new SchemaValidationErrorAccumulator();
                }

                return s_instance;
            }
        }

        private readonly List<SchemaValidationException> _schemaValidationExceptions = new List<SchemaValidationException>();

        public bool HasErrors => _schemaValidationExceptions.Any();

        public void Clear()
        {
            _schemaValidationExceptions.Clear();
        }

        public void AddError(JToken jToken, ErrorNumber errorNumber, params object[] args)
        {
            _schemaValidationExceptions.Add(new SchemaValidationException(jToken, errorNumber, args));
        }

        public SchemaValidationException ToException()
        {
            return new SchemaValidationException(_schemaValidationExceptions);
        }
    }
}
