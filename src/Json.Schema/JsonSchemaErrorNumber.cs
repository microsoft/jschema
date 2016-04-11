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
    internal enum JsonSchemaErrorNumber
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// A schema property that is required to be a string is not a string.
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
        /// The <code>additionalProperties</code> property is neither a Boolean
        /// nor an object.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "additionalProperties": 2
        /// }
        /// </code>
        /// </example>
        InvalidAdditionalPropertiesType = 2
    }
}
