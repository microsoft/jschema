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
    /// Generate the text of an interface.
    /// </summary>
    /// <remarks>
    /// Hat tip: Mike Bennett, "Generating Code with Roslyn",
    /// https://dogschasingsquirrels.com/2014/07/16/generating-code-with-roslyn/
    /// </remarks>
    public class InterfaceGenerator : ClassOrInterfaceGenerator
    {
        public InterfaceGenerator(
            PropertyInfoDictionary propertyInfoDictionary,
            JsonSchema schema,
            HintDictionary hintDictionary)
            : base(propertyInfoDictionary, schema, hintDictionary)
        {
        }

        public override BaseTypeDeclarationSyntax GenerateTypeDeclaration()
        {
            return SyntaxFactory.InterfaceDeclaration(TypeName)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        }

        public override void AddMembers()
        {
            TypeDeclaration = (TypeDeclaration as InterfaceDeclarationSyntax)
                .AddMembers(GenerateProperties());
        }

        protected override AttributeSyntax[] GeneratePropertyAttributes(string propertyName, string serializedName, bool isRequired)
        {
            return new AttributeSyntax[0];
        }

        protected override SyntaxToken[] GeneratePropertyModifiers(string propertyName)
        {
            return new SyntaxToken[0];
        }

        protected override AccessorDeclarationSyntax[] GeneratePropertyAccessors()
        {
            return new AccessorDeclarationSyntax[]
                {
                    SyntaxHelper.MakeGetAccessor()
                };
        }

        protected override bool IncludeProperty(string propertyName)
        {
            string hintDictionaryKey = MakeHintDictionaryKey(propertyName);
            PropertyModifiersHint propertyModifiersHint = HintDictionary.GetHint<PropertyModifiersHint>(hintDictionaryKey);
            if (propertyModifiersHint?.Modifiers.Count > 0)
            {
                bool isPublic = propertyModifiersHint.Modifiers.Contains(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                return isPublic;
            }

            return true;
        }

        protected override string MakeHintDictionaryKey(string propertyName)
        {
            // We want the interface to use the same hints as the class it was made from.
            // For example, if the class has an object-valued property that should really
            // be a dictionary, we want the interface to declare the property as a dictionary
            // as well. The dictionary is keyed by the name of the class+property, for
            // example, "Foo.Options". The interface name is "IFoo", so we remove the first
            // letter.
            return TypeName.Substring(1) + "." + propertyName.ToPascalCase();
        }
    }
}
