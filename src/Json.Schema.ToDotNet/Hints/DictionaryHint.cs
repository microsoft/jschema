// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        public DictionaryHint(string keyTypeName)
        {
            KeyTypeName = keyTypeName;
        }

        /// <summary>
        /// Gets the type name of the dictionary key (if null, the key type is <code>string</code>).
        /// </summary>
        public string KeyTypeName { get; }
    }
}
