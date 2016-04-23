// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Json.Schema.ToDotNet.Hints;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Generate the text of an enumerated type.
    /// </summary>
    public class EnumGenerator : TypeGenerator
    {
        public EnumGenerator(
            JsonSchema schema,
            HintDictionary hintDictionary)
            : base(schema, hintDictionary)
        {
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            return SyntaxFactory.EnumDeclaration(SyntaxFactory.Identifier(TypeName))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        public override void AddMembers()
        {
            if (Schema.Enum != null)
            {
                var enumMembers =
                        Schema.Enum.Select(
                            enumName => SyntaxFactory.EnumMemberDeclaration(
                                SyntaxFactory.Identifier(enumName.ToString().ToPascalCase())))
                            .ToArray();

                if (enumMembers.Any())
                {
                    var enumDeclaration = TypeDeclaration as EnumDeclarationSyntax;
                    TypeDeclaration = enumDeclaration.AddMembers(enumMembers);
                }
            }
        }
    }
}
