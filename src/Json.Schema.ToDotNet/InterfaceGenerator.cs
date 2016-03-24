// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public InterfaceGenerator(JsonSchema rootSchema, HintDictionary hintDictionary)
            : base(rootSchema, hintDictionary)
        {
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(JsonSchema schema)
        {
            return SyntaxFactory.InterfaceDeclaration(TypeName)
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SeparatedList(
                            CreateTypeAttributes())))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        public override void AddMembers(JsonSchema schema)
        {
            TypeDeclaration = (TypeDeclaration as InterfaceDeclarationSyntax)
                .AddMembers(GenerateProperties(schema));
        }

        protected override AttributeSyntax[] CreatePropertyAttributes(string propertyName)
        {
            return new AttributeSyntax[0];
        }

        protected override SyntaxTokenList CreatePropertyModifiers()
        {
            return default(SyntaxTokenList);
        }

        protected override IEnumerable<AccessorDeclarationSyntax> CreatePropertyAccessors()
        {
            return new AccessorDeclarationSyntax[]
                        {
                            SyntaxHelper.MakeGetAccessor()
                        };
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
