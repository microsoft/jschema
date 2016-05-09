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

        protected abstract SyntaxToken[] CreatePropertyModifiers(string propertyName);

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

            foreach (string propertyName in PropInfoDictionary.GetPropertyNames())
            {
                if (IncludeProperty(propertyName))
                {
                    propDecls.Add(CreatePropertyDeclaration(propertyName));
                }
            }

            return propDecls.ToArray();
        }

        protected virtual string MakeHintDictionaryKey(string propertyName)
        {
            return TypeName + "." + propertyName.ToPascalCase();
        }

        /// <summary>
        /// Returns a value indicating whether the specified property should be included
        /// in the generated type.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// <code>true</code> if the property specified by <paramref name="propertyName"/>
        /// should be included in the generated type; otherwise <code>false</code>.
        /// </returns>
        protected virtual bool IncludeProperty(string propertyName)
        {
            return true;
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
                .AddModifiers(CreatePropertyModifiers(propertyName))
                .AddAccessorListAccessors(CreatePropertyAccessors());

            AttributeSyntax[] attributes = CreatePropertyAttributes(propertyName, info.IsRequired);
            if (attributes.Length > 0)
            {
                propDecl = propDecl.AddAttributeLists(attributes
                    .Select(attr => SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr)))
                    .ToArray());
            }

            return propDecl.WithLeadingTrivia(
                SyntaxHelper.MakeDocComment(info.Description));
        }
    }
}
