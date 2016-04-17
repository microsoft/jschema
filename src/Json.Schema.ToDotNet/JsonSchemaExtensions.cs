// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
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
            return schema.IsStringWithFormat(FormatAttributes.Uri);
        }

        private static bool IsStringWithFormat(this JsonSchema schema, string format)
        {
            return
                schema.SafeGetType() == JTokenType.String
                && schema.Format == format;
        }

        internal static bool ShouldBeDictionary(
            this JsonSchema schema,
            string typeName,
            string propertyName,
            HintDictionary hintDictionary,
            out string keyTypeName)
        {
            keyTypeName = null;

            // Ignore any DictionaryHint that might apply to this property
            // if the property is not an object.
            if (schema.SafeGetType() != JTokenType.Object)
            {
                return false;
            }

            // Likewise, don't make this property a dictionary if it defines
            // any properties of its own
            if (schema.Properties != null && schema.Properties.Any())
            {
                return false;
            }

            // Is there a DictionaryHint that targets this property?
            if (hintDictionary == null)
            {
                return false;
            }

            string key = MakeHintDictionaryKey(typeName, propertyName);
            DictionaryHint dictionaryHint = hintDictionary.GetHint<DictionaryHint>(key);
            if (dictionaryHint == null)
            {
                return false;
            }

            keyTypeName = dictionaryHint.KeyTypeName ?? "string";
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

            string key = MakeHintDictionaryKey(typeName, propertyName);
            enumHint = hintDictionary.GetHint<EnumHint>(key);
            if (enumHint != null)
            {
                if (string.IsNullOrWhiteSpace(enumHint.TypeName))
                {
                    throw Error.CreateException(
                                    Resources.ErrorEnumHintRequiresTypeName,
                                    key);
                }

                return true;
            }

            return false;
        }

        private static string MakeHintDictionaryKey(string typeName, string propertyName)
        {
            return typeName + "." + propertyName.ToPascalCase();
        }
    }
}
