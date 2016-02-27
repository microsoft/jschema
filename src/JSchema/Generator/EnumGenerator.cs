// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generate the text of an enumerated type.
    /// </summary>
    public class EnumGenerator: TypeGenerator
    {
        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Enum != null)
            {
                foreach (string enumName in schema.Enum)
                {
                    AddEnumName(enumName);
                }
            }
        }

        public override void Finish()
        {
            throw new NotImplementedException();
        }

        private void AddEnumName(string enumName)
        {
            throw new NotImplementedException();
        }
    }
}
