// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.Json.Schema.ToDotNet
{
    public class Error
    {
        public static ApplicationException CreateException(string messageFormat, params object[] messageArgs)
        {
            return new ApplicationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    messageFormat,
                    messageArgs));
        }
    }
}
