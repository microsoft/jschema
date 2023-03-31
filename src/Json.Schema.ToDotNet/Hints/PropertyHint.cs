// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the code generator to declare a
    /// property with the specified modifiers, typeName, propertyName
    /// </summary>
    public class PropertyHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyHint"/> class.
        /// </summary>
        /// <param name="modifiers">
        /// The property modifiers.
        /// </param>
        /// <param name="typeName">
        /// The type of the .NET property to generate.
        /// </param>
        /// <param name="name">
        /// The name of the .NET property to generate.
        /// </param>
        public PropertyHint(IEnumerable<string> modifiers, string typeName, string name)
        {
            Modifiers = modifiers?.Select(TokenFromModifierName).ToList();
            TypeName = typeName;
            Name = name;
        }

        private SyntaxToken TokenFromModifierName(string modifierName)
        {
            SyntaxKind kind;

            switch (modifierName)
            {
                case "public":
                    kind = SyntaxKind.PublicKeyword;
                    break;

                case "internal":
                    kind = SyntaxKind.InternalKeyword;
                    break;

                case "protected":
                    kind = SyntaxKind.PrivateKeyword;
                    break;

                case "private":
                    kind = SyntaxKind.PrivateKeyword;
                    break;

                case "override":
                    kind = SyntaxKind.OverrideKeyword;
                    break;

                default:
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.ErrorInvalidModifier,
                            modifierName));
            }

            return SyntaxFactory.Token(kind);
        }

        /// <summary>
        /// Gets the property modifiers.
        /// </summary>
        public IList<SyntaxToken> Modifiers { get; }

        /// <summary>
        /// Gets the type of the .NET property to generate.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the name of the .NET property to generate.
        /// </summary>
        public string Name { get; }
    }
}
