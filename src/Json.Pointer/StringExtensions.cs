// Copyright (c) Microsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Json.Pointer
{
    public static class StringExtensions
    {
        public static string AtProperty(this string jPointer, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return $"{jPointer}/{propertyName.EscapeJsonPointer()}";
        }

        public static string AtIndex(this string jPointer, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return $"{jPointer}/{index}";
        }

        // The components of a JSON Pointer are separated by a '/' character. So when
        // constructing a JSON Pointer one of whose components is a property name that
        // includes the '/' character, that character must be escaped with "~1". But now
        // the '~' character is also special, so it must be escaped with "~0".
        //
        // When escaping, the "~" replacement must come first. Otherwise, the string "/"
        // would translate to "~01" instead of the correct "~1". Similarly, when
        // unescaping, the "~1" replacement must come first.
        public static string EscapeJsonPointer(this string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return propertyName.Replace("~", "~0").Replace("/", "~1");
        }

        public static string UnescapeJsonPointer(this string jPointer)
        {
            if (jPointer == null)
            {
                throw new ArgumentNullException(nameof(jPointer));
            }

            return jPointer.Replace("~1", "/").Replace("~0", "~");
        }
    }
}
