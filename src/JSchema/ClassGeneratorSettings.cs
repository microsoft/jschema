// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace MountBaker.JSchema
{
    public class ClassGeneratorSettings
    {
        public const string DefaultOutputDirectory = "Generated";

        public static ClassGeneratorSettings Default = new ClassGeneratorSettings();

        public ClassGeneratorSettings()
        {
            OutputDirectory = DefaultOutputDirectory;
        }

        public string OutputDirectory { get; set; }
    }
}
