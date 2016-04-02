// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Constructs unique names for each local variable used in the generated method
    /// implementations.
    /// </summary>
    internal class LocalVariableNameGenerator
    {
        private const string CollectionElementVariableNameBase = "value_";
        private const string LoopIndexVariableNameBase = "index_";
        private const string DestinationVariableNameBase = "destination_";
        private const string XorVariableNameBase = "xor_";

        private int _collectionElementVariableCount;
        private int _loopIndexVariableCount;
        private int _destinationVariableCount;
        private int _xorVariableCount;

        internal LocalVariableNameGenerator()
        {
            Reset();
        }

        internal void Reset()
        {
            _collectionElementVariableCount = 0;
            _loopIndexVariableCount = 0;
            _destinationVariableCount = 0;
            _xorVariableCount = 0;
        }

        internal static string GetLoopIndexVariableName(int n)
        {
            return LoopIndexVariableNameBase + n.ToString(CultureInfo.InvariantCulture);
        }

        internal static string GetCollectionElementVariableName(int n)
        {
            return CollectionElementVariableNameBase + n.ToString(CultureInfo.InvariantCulture);
        }

        internal string GetNextCollectionElementVariableName()
        {
            return CollectionElementVariableNameBase + _collectionElementVariableCount++;
        }

        internal string GetNextLoopIndexVariableName()
        {
            return LoopIndexVariableNameBase + _loopIndexVariableCount++;
        }

        internal string GetNextDestinationVariableName()
        {
            return DestinationVariableNameBase + _destinationVariableCount++;
        }

        internal string GetNextXorVariableName()
        {
            return XorVariableNameBase + _xorVariableCount++;
        }
    }
}
