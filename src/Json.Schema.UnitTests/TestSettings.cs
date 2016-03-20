// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.Json.Schema.Generator;
using Microsoft.Json.Schema.UnitTests;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    internal class TestSettings
    {
        internal const string RootClassName = "C";

        internal static DataModelGeneratorSettings MakeSettings()
        {
            return new DataModelGeneratorSettings
            {
                NamespaceName = "N",
                RootClassName = "C",
                OutputDirectory = TestFileSystem.OutputDirectory,
                ForceOverwrite = true,
                GenerateOverrides = false,
                GenerateCloningCode = false
            };
        }
    }
}
