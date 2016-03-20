// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Upper-case the first letter of a string to change it from camelCase to
        /// PascalCase.
        /// </summary>
        /// <param name="s">
        /// The string whose first letter is to be upper-cased.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="s"/> in which the first letter has been upper-cased.
        /// </returns>
        internal static string ToPascalCase(this string s)
        {
            return s[0].ToString().ToUpperInvariant() + s.Substring(1);
        }

        /// <summary>
        /// Lower-case the first letter of a string to change it from PascalCase to
        /// camelCase.
        /// </summary>
        /// <param name="s">
        /// The string whose first letter is to be lower-cased.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="s"/> in which the first letter has been lower-cased.
        /// </returns>
        internal static string ToCamelCase(this string s)
        {
            return s[0].ToString().ToLowerInvariant() + s.Substring(1);
        }
    }
}
