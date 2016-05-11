// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
                AddUsing("System");
            }

            return enumDeclaration;
        }

        public override void AddMembers()
        {
            if (Schema?.Enum.Length > 0)
            {
                EnumHint enumHint = GetEnumHintForType(TypeName);
                int[] enumValues = enumHint?.MemberValues;

                var enumDeclaration = TypeDeclaration as EnumDeclarationSyntax;

                var enumMembers = new List<EnumMemberDeclarationSyntax>();

                int enumValueIndexOffset = enumHint?.HasZeroValue == true ? 1 : 0;


                for (int i = 0; i < Schema.Enum.Length; ++i)
                {
                    EqualsValueClauseSyntax equalsValueClause = null;
                    if (enumValues != null && ShouldSupplyValueFor(i, enumHint.HasZeroValue))
                    {
                        equalsValueClause = SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(enumValues[i - enumValueIndexOffset])));
                    }

                    string enumName = Schema.Enum[i].ToString().ToPascalCase();

                    enumMembers.Add(
                        SyntaxFactory.EnumMemberDeclaration(
                            default(SyntaxList<AttributeListSyntax>),
                            SyntaxFactory.Identifier(enumName),
                            equalsValueClause));

                }

                TypeDeclaration = enumDeclaration.AddMembers(enumMembers.ToArray());
            }
        }

        /// <summary>
        /// Return a value indicating whether an explicit value should be
        /// supplied for the specified enum member.
        /// </summary>
        /// <param name="i">
        /// The zero-based index of the enum member being generated.
        /// </param>
        /// <param name="hasZeroValue">
        /// <code>true</code> if this enum type was hinted to have a zero value;
        /// otherwise <code>false</code>.
        /// </param>
        /// <returns>
        /// <code>true</code> if an explicit value should be generated for
        /// the enum member specified by <paramref name="i"/>; otherwise
        /// <code>false</code>.
        /// </returns>
        private bool ShouldSupplyValueFor(int i, bool hasZeroValue)
        {
            // If this enum has a zero value, don't supply an explicit value for it,
            // but do supply explicit values for the remaining members.
            return i > 0 || !hasZeroValue;
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
