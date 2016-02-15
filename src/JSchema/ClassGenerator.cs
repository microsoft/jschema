// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace MountBaker.JSchema
{
    /// <summary>
    /// Generates a set of .NET classes from a JSON schema.
    /// </summary>
    public static class ClassGenerator
    {
        public static void Generate(JsonSchema schema, ClassGeneratorSettings settings = null)
        {
            if (settings == null)
            {
                settings = ClassGeneratorSettings.Default;
            }

            Generate(schema, settings, new FileSystem());
        }

        internal static void Generate(JsonSchema schema, ClassGeneratorSettings settings, IFileSystem fileSystem)
        {
            if (fileSystem.DirectoryExists(settings.OutputDirectory))
            {
                throw JSchemaException.Create(Resources.ErrorOutputDirectoryExists, settings.OutputDirectory);
            }

            fileSystem.CreateDirectory(settings.OutputDirectory);
        }
    }
}
