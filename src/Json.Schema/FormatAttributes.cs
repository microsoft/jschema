// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Defines the string values for the format attributes supported by this
    /// implementation.
    /// </summary>
    /// <remarks>
    /// In the JSON Schema spec, the "format" keyword is used to define formatting
    /// requirements on string instances. The spec uses the term "attribute" to refer to
    /// the value of the "format" keyword.
    /// </remarks>
    public static class FormatAttributes
    {
        /// <summary>
        /// Format attribute specifying that the string instance must be a valid date
        /// representation as defined by RFC 3339, section 5.6.
        /// </summary>
        public const string DateTime = "date-time";

        /// <summary>
        /// Format attribute specifying that the string instance must be a valid
        /// absolute URI as defined by RFC 3986.
        /// </summary>
        public const string Uri = "uri";

        /// <summary>
        /// Format attribute specifying that the string instance must be a valid
        /// URI references as defined by RFC 3986.
        /// </summary>
        public const string UriReference = "uri-reference";
    }
}
