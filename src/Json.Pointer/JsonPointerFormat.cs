// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Json.Pointer
{
    /// <summary>
    /// Values that specify the textual format of a JSON pointer.
    /// </summary>
    public enum JsonPointerFormat
    {
        /// <summary>
        /// The syntax specified in RFC 6901, Sec. 3.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The URI fragment identifier representation specified in RFC 6901, Sec. 6.
        /// </summary>
        UriFragment = 1
    }
}