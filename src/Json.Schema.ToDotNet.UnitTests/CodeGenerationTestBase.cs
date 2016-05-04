// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    public abstract class CodeGenerationTestBase
    {
        protected TestFileSystem TestFileSystem { get; }
        protected DataModelGeneratorSettings Settings { get; }

        public CodeGenerationTestBase()
        {
            TestFileSystem = new TestFileSystem();
            Settings = TestSettings.MakeSettings();
        }
    }
}
