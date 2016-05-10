// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// a property whose type is <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
    /// rather than <see cref="System.Object"/>.
    /// </summary>
    public class DictionaryHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryHint"/> class.
        /// </summary>
        /// <param name="keyTypeName">
        /// The type name of the dictionary key (if null, the key type is <code>string</code>).
        /// </param>
        /// <param name="valueTypeName">
        /// The type name of the dictionary key (if null, the key type is <code>string</code>).
        /// </param>
        /// <param name="namespaceName">
        /// The namespace in which the type specified by <paramref name="valueTypeName"/>
        /// resides. Can be <code>null</code> if <code>valueTypeName</code> specifies a
        /// fully qualified type name.
        /// </param>
        /// <param name="comparisonKind">
        /// A string which specifies how values of this type are compared.
        /// </param>
        /// <param name="hashKind">
        /// A string which specifies how the hash code for values of this type is computed.
        /// </param>
        /// <param name="initializationKind">
        /// A string which specifies how values of this type are initialized.
        /// </param>
        public DictionaryHint(
            string keyTypeName,
            string valueTypeName,
            string namespaceName,
            string comparisonKind,
            string hashKind,
            string initializationKind)
        {
            KeyTypeName = keyTypeName;
            ValueTypeName = valueTypeName;
            NamespaceName = namespaceName;
            ComparisonKind = ParseEnum<ComparisonKind>(comparisonKind);
            HashKind = ParseEnum<HashKind>(hashKind);
            InitializationKind = ParseEnum<InitializationKind>(initializationKind);
        }

        /// <summary>
        /// Gets the type name of the dictionary key (if <code>null</code>, the key type
        /// is <code>string</code>).
        /// </summary>
        public string KeyTypeName { get; }

        /// <summary>
        /// Gets the type name of the dictionary value (if <code>null</code>, the value
        /// type is <code>string</code>).
        /// </summary>
        public string ValueTypeName { get; }

        /// <summary>
        /// Gets the in which the type specified by <see cref="ValueTypeName"/> resides.
        /// Can be <code>null</code> if <code>ValueTypeName</code> specifies a fully
        /// qualified type name.
        /// </summary>
        public string NamespaceName { get; }

        /// <summary>
        /// Gets a value which specifies how values of this type are compared.
        /// </summary>
        public ComparisonKind ComparisonKind { get; }

        /// <summary>
        /// Gets a value which specifies how the hash code for values of this type is computed.
        /// </summary>
        public HashKind HashKind { get; }

        /// <summary>
        /// Gets a value which specifies how values of this type are initialized.
        /// </summary>
        public InitializationKind InitializationKind { get; }

        private static T ParseEnum<T>(string enumString) where T: struct
        {
            T result;
            if (string.IsNullOrWhiteSpace(enumString))
            {
                result = default(T);
            }
            else if (!Enum.TryParse(enumString, out result))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.ErrorInvalidEnumValue,
                        enumString,
                        typeof(T).FullName));
            }

            return result;
        }
    }
}
