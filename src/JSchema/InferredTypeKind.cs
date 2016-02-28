// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema
{
    /// <summary>
    /// A set of values that represent the kind of a data type inferred from a JSON schema.
    /// </summary>
    /// <remarks>
    /// A schema may specify a type, which must be one of the "primitive" types defined
    /// by the JSON Schema specification. If a schema does not specify a type, it is
    /// sometimes possible to infer the type. The inferred type might be one of the
    /// primitive types, or it might be a class type whose name can be inferred. This
    /// enumeration specifies which of those two forms was inferred.
    /// </remarks>
    public enum InferredTypeKind
    {
        /// <summary>
        /// No kind was inferred.
        /// </summary>
        None,

        /// <summary>
        /// One of the primitive types defined by the JSON Schema specification was inferred.
        /// </summary>
        JsonType,

        /// <summary>
        /// The name of a class was inferred.
        /// </summary>
        ClassName
    }
}
