// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace MountBaker.JSchema.Generator
{
    /// <summary>
    /// Settings that control the operation of the <see cref="DataModelGenerator"/>.
    /// </summary>
    public class DataModelGeneratorSettings
    {
        /// <summary>
        /// The default path of the directory in which the classes will be generated.
        /// </summary>
        public const string DefaultOutputDirectory = "Generated";

        /// <summary>
        /// An instance of the <see cref="DataModelGeneratorSettings"/> class with default settings.
        /// </summary>
        public static DataModelGeneratorSettings Default = new DataModelGeneratorSettings();

        /// <summary>
        /// Creates a new instance of the <see cref="DataModelGeneratorSettings"/> class.
        /// </summary>
        public DataModelGeneratorSettings()
        {
            OutputDirectory = DefaultOutputDirectory;
        }

        /// <summary>
        /// Gets or sets the path to the directory in which the classes will be generated.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether files in the specified output directory
        /// should be overwritten.
        /// </summary>
        public bool ForceOverwrite { get; set; }

        /// <summary>
        /// Gets or sets the name of the namespace in which the classes will be generated.
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the class at the root of the generated object model
        /// </summary>
        public string RootClassName { get; set; }
    }
}
