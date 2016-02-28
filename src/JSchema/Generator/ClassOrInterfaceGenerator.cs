// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Encapsulates the commonalities between class generation and interface generation.
    /// </summary>
    public abstract class ClassOrInterfaceGenerator : TypeGenerator
    {
        protected abstract SyntaxTokenList CreatePropertyModifiers();
    }
}
