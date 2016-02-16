// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace MountBaker.JSchema.Generator
{
    /// <summary>
    /// Generates a set of .NET classes from a JSON schema.
    /// </summary>
    public static class DataModelGenerator
    {
        public static void Generate(JsonSchema schema, DataModelGeneratorSettings settings = null)
        {
            if (settings == null)
            {
                settings = DataModelGeneratorSettings.Default;
            }

            settings.Validate();

            Generate(schema, settings, new FileSystem());
        }

        internal static void Generate(JsonSchema schema, DataModelGeneratorSettings settings, IFileSystem fileSystem)
        {
            if (fileSystem.DirectoryExists(settings.OutputDirectory) && !settings.ForceOverwrite)
            {
                throw JSchemaException.Create(Resources.ErrorOutputDirectoryExists, settings.OutputDirectory);
            }

            fileSystem.CreateDirectory(settings.OutputDirectory);

            CreateFile(settings.NamespaceName, settings.RootClassName, settings.OutputDirectory);
        }

        private static void CreateFile(string namespaceName, string rootClassName, string outputDirectory)
        {
            var workspace = new AdhocWorkspace();
            Project project = workspace.AddProject("GeneratedProject", LanguageNames.CSharp);
            Document document = project.AddDocument(rootClassName, string.Empty);
            SyntaxNode root = document.GetSyntaxRootAsync().Result;
            var editor = DocumentEditor.CreateAsync(document).Result;
            var generator = SyntaxGenerator.GetGenerator(document);
            SyntaxNode namespaceDeclaration = generator.NamespaceDeclaration(namespaceName);
            editor.InsertAfter(root, namespaceDeclaration);
            document = editor.GetChangedDocument();
            SourceText sourceText = document.GetTextAsync().Result;
            System.IO.File.WriteAllText(System.IO.Path.Combine(outputDirectory, rootClassName + ".cs"), sourceText.ToString());
        }
    }
}
