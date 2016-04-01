// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Constructs unique names for each local variable used in the generated method
    /// implementations.
    /// </summary>
    internal class LocalVariableNameGenerator
    {
        private const string LoopVariableNameBase = "value_";
        private const string DestinationVariableNameBase = "destination_";
        private const string XorVariableNameBase = "xor_";

        private int _loopVariableCount;
        private int _destinationVariableCount;
        private int _xorVariableCount;

        internal LocalVariableNameGenerator()
        {
            Reset();
        }

        internal void Reset()
        {
            _loopVariableCount = 0;
            _destinationVariableCount = 0;
            _xorVariableCount = 0;
        }

        internal string GetNextLoopVariableName()
        {
            return LoopVariableNameBase + _loopVariableCount++;
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
