// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Values specifying each of the supported kinds of code generation hints.
    /// </summary>
    /// <remarks>
    /// This enumeration exists to avoid arbitrary code execution attacks by way of the
    /// HintDictionary. Rather than the code generation hints file specifying arbitrary
    /// classes to instantiate, it instead specifies one of a fixed number of supported
    /// hints, along with values that parameterize specific instances of those hints.
    /// The <see cref="HintDictionary"/> class deserializes the hints file and
    /// explicitly instantiates only those <see cref="CodeGenHint"/>-derived classes
    /// that it supports.
    /// </remarks>
    public enum HintKind
    {
        None = 0,
        AttributeHint,
        BaseTypeHint,
        ClassNameHint,
        DictionaryHint,
        EnumHint,
        InterfaceHint,
        PropertyModifiersHint,
        PropertyNameHint
    }
}
