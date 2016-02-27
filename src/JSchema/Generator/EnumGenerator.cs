// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.JSchema.Generator
{
    /// <summary>
    /// Generate the text of an enumerated type.
    /// </summary>
    public class EnumGenerator: TypeGenerator
    {
        public override void AddMembers(JsonSchema schema)
        {
            if (schema.Enum != null)
            {
                foreach (string enumName in schema.Enum)
                {
                    AddEnumName(enumName);
                }
            }
        }

        public override BaseTypeDeclarationSyntax CreateTypeDeclaration(string typeName)
        {
            return SyntaxFactory.EnumDeclaration(SyntaxFactory.Identifier(typeName));
        }

        public override void Finish()
        {
            base.Finish();
        }

        private void AddEnumName(string enumName)
        {
            throw new NotImplementedException();
        }
    }
}
