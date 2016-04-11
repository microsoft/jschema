// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Error numbers from validating a JSON instance against a JSON schema.
    /// </summary>
    /// <remarks>
    /// Do not alter or reuse the integer values used in the enum initializers.
    /// They must remain stable because the error codes will be documented.
    /// </remarks>
    internal enum ValidationErrorNumber
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// A token has the wrong type.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer"
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "x"
        /// </code>
        /// </example>
        WrongType = 1,

        /// <summary>
        /// A required property is missing.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "required": [ "a", "b", "c" ]
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// {
        ///   "a": 1,
        ///   "b": 2
        /// }
        /// </code>
        /// </example>
        RequiredPropertyMissing = 2,

        /// <summary>
        /// An array has too few items.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "array",
        ///   "minItems": 3
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// [ 1, 2 ]
        /// </code>
        /// </example>
        TooFewArrayItems,

        /// <summary>
        /// An array has too many items.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "array",
        ///   "minItems": 3
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// [ 1, 2, 3, 4 ]
        /// </code>
        /// </example>
        TooManyArrayItems,

        /// <summary>
        /// An object has a property not permitted by the schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// {
        ///   "a": 3
        /// }
        /// </code>
        /// </example>
        AdditionalPropertiesProhibited,
    }
}