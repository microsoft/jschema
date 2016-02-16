// Copyright (c) Mount Baker Software.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace MountBaker.JSchema.Generator
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

            CreateFile(_settings.RootClassName);
        }

        private void CreateFile(string className)
        {
            var workspace = new AdhocWorkspace();

            // Hat tip: Mike Bennett, "Generating Code with Roslyn",
            // https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
            CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit();

            NamespaceDeclarationSyntax ns =
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_settings.NamespaceName));

            ClassDeclarationSyntax cls = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            ns = ns.AddMembers(cls);
            cu = cu.AddMembers(ns);

            SyntaxNode formattedNode = Formatter.Format(cu, workspace);
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            _fileSystem.WriteAllText(Path.Combine(_settings.OutputDirectory, className + ".cs"), sb.ToString());
        }
    }
}
