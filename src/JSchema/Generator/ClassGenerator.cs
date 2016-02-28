// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
    public class ClassGenerator : ClassOrInterfaceGenerator
    {
        private readonly string _interfaceName;

        public ClassGenerator(JsonSchema rootSchema, string interfaceName)
            : base(rootSchema)
        {
            _interfaceName = interfaceName;
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(string typeName)
        {
            var modifiers = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            var classDeclaration = SyntaxFactory.ClassDeclaration(typeName).WithModifiers(modifiers);

            if (_interfaceName != null)
            {
                SimpleBaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(_interfaceName));
                SeparatedSyntaxList<BaseTypeSyntax> separatedBaseList = SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType);
                BaseListSyntax baseList = SyntaxFactory.BaseList(separatedBaseList);
                classDeclaration = classDeclaration.WithBaseList(baseList);
            }

            return classDeclaration;
        }

        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                SyntaxList<MemberDeclarationSyntax> members = CreateProperties(schema);
                TypeDeclaration = (TypeDeclaration as ClassDeclarationSyntax).WithMembers(members);
            }
        }

        protected override SyntaxTokenList CreatePropertyModifiers()
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (_interfaceName != null)
            {
                modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            }

            return modifiers;
        }
    }
}
