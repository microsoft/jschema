// Copyright (c) Microsoft Corporation.  All Rights Reserved. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generates a set of .NET classes from a JSON schema.
    /// </summary>
    public class DataModelGenerator
    {
        private readonly DataModelGeneratorSettings _settings;
        private readonly IFileSystem _fileSystem;
        private JsonSchema _rootSchema;
        private HintDictionary _hintDictionary;

        public DataModelGenerator(DataModelGeneratorSettings settings)
            : this(settings, new FileSystem())
        {
        }

        // This ctor allows unit tests to mock the file system.
        internal DataModelGenerator(DataModelGeneratorSettings settings, IFileSystem fileSystem)
        {
            _settings = settings;
            _settings.Validate();

            _fileSystem = fileSystem;
        }

        public string Generate(JsonSchema rootSchema, HintDictionary hintDictionary = null)
        {
            _rootSchema = rootSchema;
            _hintDictionary = hintDictionary;

            if (_fileSystem.DirectoryExists(_settings.OutputDirectory) && !_settings.ForceOverwrite)
            {
                throw JSchemaException.Create(Resources.ErrorOutputDirectoryExists, _settings.OutputDirectory);
            }

            _fileSystem.CreateDirectory(_settings.OutputDirectory);

            if (_rootSchema.Type != JsonType.Object)
            {
                throw JSchemaException.Create(Resources.ErrorNotAnObject, _rootSchema.Type);
            }

            if (_settings.CopyrightFilePath != null && !_fileSystem.FileExists(_settings.CopyrightFilePath))
            {
                throw JSchemaException.Create(Resources.ErrorCopyrightFileNotFound, _settings.CopyrightFilePath);
            }

            string copyrightNotice = _fileSystem.ReadAllText(_settings.CopyrightFilePath);

            string rootFileText = CreateFile(_settings.RootClassName, _rootSchema, copyrightNotice);

            if (_rootSchema.Definitions != null)
            {
                foreach (KeyValuePair<string, JsonSchema> definition in _rootSchema.Definitions)
                {
                    CreateFile(definition.Key, definition.Value, copyrightNotice);
                }
            }

            // Returning the text of the file generated from the root schema allows this method
            // to be more easily unit tested.
            return rootFileText;
        }

        internal string CreateFile(string className, JsonSchema schema, string copyrightNotice = null)
        {
            className = className.ToPascalCase();

            string text = CreateFileText(className, schema, copyrightNotice);
            _fileSystem.WriteAllText(Path.Combine(_settings.OutputDirectory, className + ".cs"), text);

            return text;
        }

        internal string CreateFileText(string className, JsonSchema schema, string copyrightNotice = null)
        {
            CodeGenHint[] hints = null;
            EnumHint enumHint = null;
            if (_hintDictionary != null && _hintDictionary.TryGetValue(className.ToCamelCase(), out hints))
            {
                enumHint = hints.First(h => h is EnumHint) as EnumHint;
            }

            TypeGenerator typeGenerator;
            if (enumHint == null)
            {
                typeGenerator = new ClassGenerator(_rootSchema);
            }
            else
            {
                typeGenerator = new EnumGenerator();
            }

            typeGenerator.Start(_settings.NamespaceName, className.ToPascalCase(), copyrightNotice, schema.Description);
            typeGenerator.AddMembers(schema);
            typeGenerator.Finish();

            return typeGenerator.GetText();
        }
    }
}
