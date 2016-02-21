// Copyright (c) Microsoft Corporation.  All Rights Reserved. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generates a set of .NET classes from a JSON schema.
    /// </summary>
    public class DataModelGenerator
    {
        private readonly DataModelGeneratorSettings _settings;
        private readonly IFileSystem _fileSystem;

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

        public void Generate(JsonSchema schema)
        {
            if (_fileSystem.DirectoryExists(_settings.OutputDirectory) && !_settings.ForceOverwrite)
            {
                throw JSchemaException.Create(Resources.ErrorOutputDirectoryExists, _settings.OutputDirectory);
            }

            _fileSystem.CreateDirectory(_settings.OutputDirectory);

            if (schema.Type != JsonType.Object)
            {
                throw JSchemaException.Create(Resources.ErrorNotAnObject, schema.Type);
            }

            if (_settings.CopyrightFilePath != null && !File.Exists(_settings.CopyrightFilePath))
            {
                throw JSchemaException.Create(Resources.ErrorCopyrightFileNotFound, _settings.CopyrightFilePath);
            }

            string copyrightNotice = _fileSystem.ReadAllText(_settings.CopyrightFilePath);

            CreateFile(_settings.RootClassName, schema, copyrightNotice);
        }

        internal void CreateFile(string className, JsonSchema schema, string copyrightNotice = null)
        {
            className = className.ToPascalCase();

            string text = CreateFileText(className, schema, copyrightNotice);
            _fileSystem.WriteAllText(Path.Combine(_settings.OutputDirectory, className + ".cs"), text);
        }

        // TODO: We refactored CreateFileText from CreateFile to separate out the file system handling.
        // But CreateFileText turns right around an calls CreateFile, so we didn't help at all.
        // Refactor/untangle this.
        internal string CreateFileText(string className, JsonSchema schema, string copyrightNotice = null)
        {
            var classGenerator = new ClassGenerator();
            classGenerator.StartClass(_settings.NamespaceName, className.ToPascalCase(), copyrightNotice);

            if (schema.Properties != null)
            {
                foreach (KeyValuePair<string, JsonSchema> schemaProperty in schema.Properties)
                {
                    string propertyName = schemaProperty.Key;
                    JsonSchema subSchema = schemaProperty.Value;

                    if (subSchema.Type == JsonType.Object)
                    {
                        // TODO: We haven't unit tested this path.
                        CreateFile(propertyName, subSchema, copyrightNotice);
                    }

                    JsonType propertyType = GetEffectiveType(subSchema);

                    JsonType elementType = subSchema.Type == JsonType.Array
                        ? GetElementType(subSchema)
                        : JsonType.None;

                    classGenerator.AddProperty(propertyName, subSchema.Description, propertyType, elementType);
                }
            }

            if (schema.Definitions != null)
            {
                foreach (KeyValuePair<string, JsonSchema> definition in schema.Definitions)
                {
                    if (definition.Value.Type == JsonType.Object)
                    {
                        CreateFile(definition.Key, definition.Value, copyrightNotice);
                    }
                }
            }

            classGenerator.FinishClass();
            return classGenerator.GetText();
        }

        // If the current schema is of array type, get the type of
        // its elements.
        private JsonType GetElementType(JsonSchema subSchema)
        {
            return subSchema.Items != null
                ? GetEffectiveType(subSchema.Items)
                : JsonType.Object;
        }

        // Not every subschema specifies a type, but in some cases, it can be inferred.
        private JsonType GetEffectiveType(JsonSchema subSchema)
        {
            JsonType effectiveType = subSchema.Type;

            if (subSchema.Type == JsonType.None)
            {
                // If there is an enum and every value has the same type, use that.
                object[] enumVals = subSchema.Enum;
                if (enumVals != null && enumVals.Length > 0)
                {
                    effectiveType = GetJsonTypeFromObject(enumVals[0]);
                    for (int i = 1; i < enumVals.Length; ++i)
                    {
                        if (GetJsonTypeFromObject(enumVals[i]) != effectiveType)
                        {
                            effectiveType = JsonType.None;
                            break;
                        }
                    }
                }
            }

            return effectiveType;
        }

        private JsonType GetJsonTypeFromObject(object obj)
        {
            if (obj is string)
            {
                return JsonType.String;
            }
            else if (obj.IsIntegralType())
            {
                return JsonType.Integer;
            }
            else if (obj.IsFloatingType())
            {
                return JsonType.Number;
            }
            else if (obj is bool)
            {
                return JsonType.Boolean;
            }
            else
            {
                return JsonType.None;
            }
        }
    }
}
