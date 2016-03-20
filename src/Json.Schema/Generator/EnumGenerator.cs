// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.Generator
{
    /// <summary>
    /// Generate the text of an enumerated type.
    /// </summary>
    public class EnumGenerator : TypeGenerator
    {
        public EnumGenerator(HintDictionary hintDictionary)
            : base(hintDictionary)
        {
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(JsonSchema schema)
        {
            return SyntaxFactory.EnumDeclaration(SyntaxFactory.Identifier(TypeName))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }

        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Enum != null)
            {
                var enumMemberDeclarations = new List<EnumMemberDeclarationSyntax>(
                        schema.Enum.Select(
                            enumName => SyntaxFactory.EnumMemberDeclaration(
                                SyntaxFactory.Identifier(enumName.ToString().ToPascalCase()))));

                if (enumMemberDeclarations.Any())
                {
                    SeparatedSyntaxList<EnumMemberDeclarationSyntax> enumMemberList =
                        SyntaxFactory.SeparatedList(enumMemberDeclarations);

                    var enumDeclaration = TypeDeclaration as EnumDeclarationSyntax;
                    TypeDeclaration = enumDeclaration.WithMembers(enumMemberList);
                }
            }
        }
    }
}
