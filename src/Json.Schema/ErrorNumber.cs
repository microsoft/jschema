// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Error numbers from reading a JSON schema.
    /// </summary>
    /// <remarks>
    /// Do not alter or reuse the integer values used in the enum initializers.
    /// They must remain stable because the error codes will be documented.
    /// </remarks>
    internal enum ErrorNumber
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// In the schema, a property that is required to be a string is not a string.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "title": 2
        /// }
        /// </code>
        /// </example>
        NotAString = 1,

        /// <summary>
        /// In the schema, <code>additionalProperties</code> property is neither a Boolean
        /// nor an object.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "additionalProperties": 2
        /// }
        /// </code>
        /// </example>
        InvalidAdditionalPropertiesType = 2,

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
        WrongType = 3,

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
        RequiredPropertyMissing = 4,

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
        TooFewArrayItems = 5,

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
        TooManyArrayItems = 6,

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
        AdditionalPropertiesProhibited = 7,

        /// <summary>
        /// A numeric instance has a value greater than the maximum value permitted by the
        /// schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer",
        ///   "maximum": 2
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "3"
        /// </code>
        /// </example>
        ValueTooLarge = 8,

        /// <summary>
        /// A numeric instance has a value greater than or equal to the exclusive maximum
        /// value permitted by the schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer",
        ///   "maximum": 2,
        ///   "exclusiveMaximum": true
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "2"
        /// </code>
        /// </example>
        ValueTooLargeExclusive = 9,
    }
}
