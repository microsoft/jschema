// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class JsonSchemaExtensions
    {
        internal static bool IsDateTime(this JsonSchema schema)
        {
            return schema.Type == JsonType.String && schema.Format == FormatAttributes.DateTime;
        }

        internal static bool IsUri(this JsonSchema schema)
        {
            return schema.Type == JsonType.String && schema.Format == FormatAttributes.Uri;
        }

        internal static bool ShouldBeDictionary(
            this JsonSchema schema,
            string typeName,
            string propertyName,
            HintDictionary hintDictionary)
        {
            // Ignore any DictionaryHint that might apply to this property
            // if the property is not an object.
            if (schema.Type != JsonType.Object)
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
            string key = MakeHintDictionaryKey(typeName, propertyName);

            return hintDictionary != null
                && hintDictionary.Any(
                    kvp => kvp.Key.Equals(key)
                    && kvp.Value.Any(hint => hint is DictionaryHint));
        }

        internal static bool ShouldBeEnum(
            this JsonSchema schema,
            string typeName,
            string propertyName,
            HintDictionary hintDictionary,
            out EnumHint enumHint)
        {
            bool shouldBeEnum = false;
            enumHint = null;

            string propertyKey = MakeHintDictionaryKey(typeName, propertyName);
            if (hintDictionary != null)
            {
                CodeGenHint[] hints;
                if (hintDictionary.TryGetValue(propertyKey, out hints))
                {
                    enumHint = hints.FirstOrDefault(hint => hint is EnumHint) as EnumHint;
                    if (enumHint != null)
                    {
                        if (string.IsNullOrWhiteSpace(enumHint.TypeName))
                        {
                            throw JSchemaException.Create(
                                Resources.ErrorEnumHintRequiresTypeName,
                                propertyKey);
                        }

                        shouldBeEnum = true;
                    }
                }
            }

            return shouldBeEnum;
        }

        private static string MakeHintDictionaryKey(string typeName, string propertyName)
        {
            return typeName + "." + propertyName.ToPascalCase();
        }
    }
}
