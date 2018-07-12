// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.Json.Schema.ToDotNet.Hints;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class JsonSchemaExtensions
    {
        internal static bool IsDateTime(this JsonSchema schema)
        {
            return schema.IsStringWithFormat(FormatAttributes.DateTime);
        }

        internal static bool IsUri(this JsonSchema schema)
        {
            return
                schema.IsStringWithFormat(FormatAttributes.Uri) ||
                schema.IsStringWithFormat(FormatAttributes.UriReference);
        }

        private static bool IsStringWithFormat(this JsonSchema schema, string format)
        {
            return
                schema.SafeGetType() == SchemaType.String
                && schema.Format == format;
        }

        internal static bool ShouldBeDictionary(
            this JsonSchema schema,
            string typeName,
            string propertyName,
            HintDictionary hintDictionary,
            out DictionaryHint dictionaryHint)
        {
            dictionaryHint = null;

            // Ignore any DictionaryHint that might apply to this property
            // if the property is not an object.
            if (schema.SafeGetType() != SchemaType.Object)
            {
                return false;
            }

            // Is there a DictionaryHint that targets this property?
            if (hintDictionary == null)
            {
                return false;
            }

            dictionaryHint = hintDictionary.GetPropertyHint<DictionaryHint>(typeName, propertyName);
            if (dictionaryHint == null)
            {
                return false;
            }

            return true;
        }

        internal static bool ShouldBeEnum(
            this JsonSchema schema,
            string typeName,
            string propertyName,
            HintDictionary hintDictionary,
            out EnumHint enumHint)
        {
            enumHint = null;

            if (hintDictionary == null)
            {
                return false;
            }

            enumHint = hintDictionary.GetPropertyHint<EnumHint>(typeName, propertyName);
            if (enumHint != null)
            {
                if (string.IsNullOrWhiteSpace(enumHint.TypeName))
                {
                    throw Error.CreateException(
                                    Resources.ErrorEnumHintRequiresTypeName,
                                    typeName,
                                    propertyName);
                }

                return true;
            }

            return false;
        }
    }
}
