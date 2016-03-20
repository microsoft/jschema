// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to create
    /// a property whose type is <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
    /// rather than <see cref="System.Object"/>.
    /// </summary>
    public class DictionaryHint: CodeGenHint
    {
    }
}
