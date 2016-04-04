// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

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

        /// <summary>
        /// Extracts the property name from a string which encodes a property name together
        /// with its array rank and an indication of whether the property is a dictionary.
        /// </summary>
        /// <param name="decoratedPropertyName">
        /// A string that encodes a property name together with its array rank, for example,
        /// <code>Location[][]</code>.
        /// </param>
        /// <param name="arrayRank">
        /// The rank of the array encoded by <paramref name="decoratedPropertyName"/>.
        /// </param>
        /// <param name="isDictionary">
        /// <code>true</code> if the type encoded by <paramref name="decoratedPropertyName"/>
        /// is a dictionary; otherwise <code>false</code>.
        /// </param>
        /// <returns>
        /// The property name encoded by the string <paramref name="decoratedPropertyName"/>.
        /// </returns>
        /// <example>
        /// <para>
        /// Given <code>Location[][]</code>, this method returns <code>Location</code>,
        /// sets the value of the <code>out</code> parameter <code>arrayRank</code> to 2,
        /// and sets the value of the <code>out</code> parameter <code>isDictionary</code>
        /// to <code>false</code>.
        /// </para>
        /// <para>
        /// Given <code>Location{}[]</code>, this method returns <code>Location</code>,
        /// sets the value of the <code>out</code> parameter <code>arrayRank</code> to 1,
        /// and sets the value of the <code>out</code> parameter <code>isDictionary</code>
        /// to <code>true</code>.
        /// </para>
        /// </example>
        internal static string BasePropertyName(this string decoratedPropertyName, out int arrayRank, out bool isDictionary)
        {
            arrayRank = 0;
            isDictionary = false;

            string propertyName = decoratedPropertyName;
            while (propertyName.EndsWith(PropertyInfoDictionary.ArrayMarker))
            {
                ++arrayRank;
                propertyName = propertyName.Substring(0, propertyName.Length - PropertyInfoDictionary.ArrayMarker.Length);
            }

            if (propertyName.EndsWith(PropertyInfoDictionary.DictionaryMarker))
            {
                isDictionary = true;
                propertyName = propertyName.Substring(0, propertyName.Length - PropertyInfoDictionary.DictionaryMarker.Length);
            }

            if (propertyName.EndsWith(PropertyInfoDictionary.ArrayMarker) ||
                propertyName.EndsWith(PropertyInfoDictionary.ArrayMarker))
            {
                throw new ArgumentException(
                    $"Cannot generate code for property {decoratedPropertyName} because it is not an array, a dictionary of scalars, or a dictionary of arrays.");
            }

            return propertyName;
        }
    }
}
