// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace MountBaker.JSchema.Generator
{
    public class DataModelGeneratorSettings
    {
        public const string DefaultOutputDirectory = "Generated";

        public static DataModelGeneratorSettings Default = new DataModelGeneratorSettings();

        public DataModelGeneratorSettings()
        {
            OutputDirectory = DefaultOutputDirectory;
        }

        public string OutputDirectory { get; set; }

        public bool ForceOverwrite { get; set; }
    }
}
