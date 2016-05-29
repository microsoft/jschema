// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Comparer class to sort <code>using</code> directives with <code>System</code>
    /// namespaces first.
    /// </summary>
    internal class UsingComparer : IComparer<string>
    {
        internal static readonly UsingComparer Instance = new UsingComparer();

        public int Compare(string first, string second)
        {
            bool firstIsSystemUsing = IsSystemUsing(first);
            bool secondIsSystemUsing = IsSystemUsing(second);

            if (firstIsSystemUsing && !secondIsSystemUsing)
            {
                return -1;
            }

            if (!firstIsSystemUsing && secondIsSystemUsing)
            {
                return 1;
            }

            return first.CompareTo(second);
        }

        private const string SystemNamespaceName = "System";
        private const string SystemUsingPrefix = SystemNamespaceName + ".";

        private static bool IsSystemUsing(string namespaceName)
        {
            return namespaceName.Equals(SystemNamespaceName)
                || namespaceName.StartsWith(SystemUsingPrefix);
        }
    }
}
