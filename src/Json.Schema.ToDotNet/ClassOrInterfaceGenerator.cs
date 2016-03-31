// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Encapsulates the commonalities between class generation and interface generation.
    /// </summary>
    public abstract class ClassOrInterfaceGenerator : TypeGenerator
    {
        public ClassOrInterfaceGenerator(
            PropertyInfoDictionary propertyInfoDictionary,
            JsonSchema schema,
            HintDictionary hintDictionary)
            : base(schema, hintDictionary)
        {
            PropInfoDictionary = propertyInfoDictionary;
        }

        protected abstract AttributeSyntax[] CreatePropertyAttributes(string propertyName, bool isRequired);

        protected abstract SyntaxToken[] CreatePropertyModifiers();

        protected abstract AccessorDeclarationSyntax[] CreatePropertyAccessors();

        /// <summary>
        /// Gets a dictionary that maps the name of each property in the generated class
        /// to a information about the property derived from the JSON schema.
        /// </summary> 
        protected PropertyInfoDictionary PropInfoDictionary { get; private set; }
        
        protected MemberDeclarationSyntax[] GenerateProperties()
        {

            IEnumerable<string> namespaceNames = PropInfoDictionary
                .Select(kvp => kvp.Value.NamespaceName)
                .Where(n => n != null)
                .Distinct()
                .OrderBy(n => n);

            foreach (string namespaceName in namespaceNames)
            {
                AddUsing(namespaceName);
            }

            var propDecls = new List<MemberDeclarationSyntax>();

            foreach (string propertyName in GetPropertyNames())
            {
                propDecls.Add(CreatePropertyDeclaration(propertyName));
            }

            return propDecls.ToArray();
        }

        protected virtual string MakeHintDictionaryKey(string propertyName)
        {
            return TypeName + "." + propertyName.ToPascalCase();
        }

        /// <summary>
        /// Gets the list of all properties declared in the schema.
        /// </summary>
        /// <remarks>
        /// Don't include information about array elements. For example, if the class has
        /// an array-valued property ArrayProp, then include "ArrayProp" in the list, but
        /// not "ArrayProp[]".
        /// </remarks>
        /// <returns>
        /// An array containing the names of the properties.
        /// </returns>
        protected string[] GetPropertyNames()
        {
            return PropInfoDictionary.Keys
                .Where(key => key.IndexOf("[]") == -1)
                .OrderBy(key => PropInfoDictionary[key].DeclarationOrder)
                .Select(key => key.ToPascalCase())
                .ToArray();
        }

        /// <summary>
        /// Create a property declaration.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// A property declaration built from the specified schema.
        /// </returns>
        private PropertyDeclarationSyntax CreatePropertyDeclaration(string propertyName)
        {
            PropertyInfo info = PropInfoDictionary[propertyName];

            PropertyDeclarationSyntax propDecl = SyntaxFactory.PropertyDeclaration(
                info.Type,
                propertyName.ToPascalCase())
                .AddModifiers(CreatePropertyModifiers())
                .AddAccessorListAccessors(CreatePropertyAccessors());

            AttributeSyntax[] attributes = CreatePropertyAttributes(propertyName, info.IsRequired);
            if (attributes.Length > 0)
            {
                propDecl = propDecl.AddAttributeLists(
                    new AttributeListSyntax[]
                    {
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SeparatedList(attributes))
                    });
            }

            return propDecl.WithLeadingTrivia(
                SyntaxHelper.MakeDocComment(info.Description));
        }
    }
}
