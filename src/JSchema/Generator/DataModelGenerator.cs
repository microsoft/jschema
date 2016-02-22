// Copyright (c) Microsoft Corporation.  All Rights Reserved. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

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

        public void Generate(JsonSchema rootSchema)
        {
            if (_fileSystem.DirectoryExists(_settings.OutputDirectory) && !_settings.ForceOverwrite)
            {
                throw JSchemaException.Create(Resources.ErrorOutputDirectoryExists, _settings.OutputDirectory);
            }

            _fileSystem.CreateDirectory(_settings.OutputDirectory);

            if (rootSchema.Type != JsonType.Object)
            {
                throw JSchemaException.Create(Resources.ErrorNotAnObject, rootSchema.Type);
            }

            if (_settings.CopyrightFilePath != null && !File.Exists(_settings.CopyrightFilePath))
            {
                throw JSchemaException.Create(Resources.ErrorCopyrightFileNotFound, _settings.CopyrightFilePath);
            }

            string copyrightNotice = _fileSystem.ReadAllText(_settings.CopyrightFilePath);

            CreateFile(_settings.RootClassName, rootSchema, rootSchema, copyrightNotice);

            if (rootSchema.Definitions != null)
            {
                foreach (KeyValuePair<string, JsonSchema> definition in rootSchema.Definitions)
                {
                    CreateFile(definition.Key, definition.Value, rootSchema, copyrightNotice);
                }
            }
        }

        internal void CreateFile(string className, JsonSchema schema, JsonSchema rootSchema, string copyrightNotice = null)
        {
            className = className.ToPascalCase();

            string text = CreateFileText(className, schema, rootSchema, copyrightNotice);
            _fileSystem.WriteAllText(Path.Combine(_settings.OutputDirectory, className + ".cs"), text);
        }

        internal string CreateFileText(string className, JsonSchema schema, JsonSchema rootSchema, string copyrightNotice = null)
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
                        CreateFile(propertyName, subSchema, rootSchema, copyrightNotice);
                    }

                    InferredType propertyType = InferTypeFromSchema(rootSchema, subSchema);

                    InferredType elementType = subSchema.Type == JsonType.Array
                        ? GetElementType(rootSchema, subSchema)
                        : InferredType.None;

                    classGenerator.AddProperty(propertyName, subSchema.Description, propertyType, elementType);
                }
            }

            classGenerator.FinishClass();
            return classGenerator.GetText();
        }

        // If the current schema is of array type, get the type of
        // its elements.
        // TODO: I'm not handling arrays of arrays. InferredType should encapsulate that.
        private InferredType GetElementType(JsonSchema rootSchema, JsonSchema subSchema)
        {
            return subSchema.Items != null
                ? InferTypeFromSchema(rootSchema, subSchema.Items)
                : new InferredType(JsonType.Object);
        }

        // Not every subschema specifies a type, but in some cases, it can be inferred.
        private InferredType InferTypeFromSchema(JsonSchema rootSchema, JsonSchema subSchema)
        {
            if (subSchema.Type != JsonType.None)
            {
                return new InferredType(subSchema.Type);
            }

            // If there is a reference, use the type of the reference.
            if (subSchema.Reference != null)
            {
                return InferTypeFromReference(rootSchema, subSchema);
            }

            // If there is an enum and every value has the same type, use that.
            object[] enumValues = subSchema.Enum;
            if (enumValues != null && enumValues.Length > 0)
            {
                var inferredType = InferTypeFromEnumValues(enumValues);
                if (inferredType != InferredType.None)
                {
                    return inferredType;
                }
            }

            return InferredType.None;
        }

        private InferredType InferTypeFromReference(JsonSchema rootSchema, JsonSchema subSchema)
        {
            if (!subSchema.Reference.IsFragment)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorOnlyDefinitionFragmentsSupported, subSchema.Reference), nameof(subSchema));
            }

            string definitionName = GetDefinitionNameFromFragment(subSchema.Reference.Fragment);

            JsonSchema definitionSchema;
            if (!rootSchema.Definitions.TryGetValue(definitionName, out definitionSchema))
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorDefinitionDoesNotExist, definitionName));
            }

            return new InferredType(definitionName.ToPascalCase());
        }

        private static readonly Regex s_definitionRegex = new Regex(@"^#/definitions/(?<definitionName>[^/]+)$");

        private string GetDefinitionNameFromFragment(string fragment)
        {
            Match match = s_definitionRegex.Match(fragment);
            if (!match.Success)
            {
                throw new JSchemaException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorOnlyDefinitionFragmentsSupported, fragment));
            }

            return match.Groups["definitionName"].Captures[0].Value;
        }

        private InferredType InferTypeFromEnumValues(object[] enumValues)
        {
            var jsonType = GetJsonTypeFromObject(enumValues[0]);
            for (int i = 1; i < enumValues.Length; ++i)
            {
                if (GetJsonTypeFromObject(enumValues[i]) != jsonType)
                {
                    jsonType = JsonType.None;
                    break;
                }
            }

            if (jsonType != JsonType.None)
            {
                return new InferredType(jsonType);
            }

            return InferredType.None;
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
