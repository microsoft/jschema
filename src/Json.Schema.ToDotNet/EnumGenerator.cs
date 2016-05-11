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
        private const string FlagsAttributeName = "Flags";

        public EnumGenerator(
            JsonSchema schema,
            HintDictionary hintDictionary)
            : base(schema, hintDictionary)
        {
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var enumDeclaration = SyntaxFactory.EnumDeclaration(SyntaxFactory.Identifier(TypeName))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            EnumHint enumHint = GetEnumHintForType(TypeName);
            if (enumHint?.Flags == true)
            {
                enumDeclaration = AddAttribute(enumDeclaration, FlagsAttributeName);
            }

            return enumDeclaration;
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

        private EnumHint GetEnumHintForType(string typeName)
        {
            return HintDictionary
                .SelectMany(kvp => kvp.Value)
                .Where(hint => hint is EnumHint)
                .Cast<EnumHint>()
                .Where(eh => eh.TypeName == typeName)
                .FirstOrDefault();
        }

        private EnumDeclarationSyntax AddAttribute(EnumDeclarationSyntax enumDeclaration, string attributeName)
        {
            return enumDeclaration.AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName(attributeName)))));
        }
    }
}
