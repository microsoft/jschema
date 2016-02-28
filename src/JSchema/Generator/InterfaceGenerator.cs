// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generate the text of a class.
    /// </summary>
    /// <remarks>
    /// Hat tip: Mike Bennett, "Generating Code with Roslyn",
    /// https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
    /// </remarks>
    public class InterfaceGenerator : ClassOrInterfaceGenerator
    {
        public InterfaceGenerator(JsonSchema rootSchema)
            : base(rootSchema)
        {
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(string typeName)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            return SyntaxFactory.InterfaceDeclaration(typeName).WithModifiers(modifiers);
        }

        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                SyntaxList<MemberDeclarationSyntax> members = CreateProperties(schema);
                TypeDeclaration = (TypeDeclaration as InterfaceDeclarationSyntax).WithMembers(members);
            }
        }

        protected override SyntaxTokenList CreatePropertyModifiers()
        {
            return default(SyntaxTokenList);
        }
    }
}
