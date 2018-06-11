// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    internal static class Utilities
    {
        internal static string QualifyNameWithSuffix(string name, string suffix)
        {
            return string.IsNullOrWhiteSpace(suffix)
                ? name
                : name + "." + suffix.Trim();
        }
    }
}
