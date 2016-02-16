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

            CreateFile(settings.NamespaceName, settings.RootClassName, settings.OutputDirectory, fileSystem);
        }

        private static void CreateFile(string namespaceName, string className, string outputDirectory, IFileSystem fileSystem)
        {
            var workspace = new AdhocWorkspace();

            // Hat tip: Mike Bennett, "Generating Code with Roslyn",
            // https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
            CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit();

            NamespaceDeclarationSyntax ns =
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceName));

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

            fileSystem.WriteAllText(Path.Combine(outputDirectory, className + ".cs"), sb.ToString());
        }
    }
}
