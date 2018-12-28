// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Json.Schema.ToDotNet.Hints;

namespace Microsoft.Json.Schema.ToDotNet
{
    public abstract class TypeGenerator
    {
        private readonly string _typeNameSuffix;

        protected TypeGenerator(
            JsonSchema schema,
            string typeNameSuffix,
            HintDictionary hintDictionary)
        {
            Schema = schema;
            HintDictionary = hintDictionary;
            _typeNameSuffix = typeNameSuffix;
        }

        protected string TypeName { get; private set; }

        public string SuffixedTypeName
        {
            get { return TypeName + _typeNameSuffix; }
        }

        protected HintDictionary HintDictionary { get; }

        protected JsonSchema Schema { get; }

        /// <summary>
        /// Gets or sets the type declaration being generated.
        /// </summary>
        protected BaseTypeDeclarationSyntax TypeDeclaration { get; set; }

        protected List<string> Usings { get; private set; }

        public abstract BaseTypeDeclarationSyntax GenerateTypeDeclaration();

        /// <summary>
        /// Adds members to the type as directed by the schema.
        /// </summary>
        public abstract void AddMembers();

        /// <summary>
        /// Generate the text for a type from a JSON schema.
        /// </summary>
        /// <param name="namespaceName">
        /// The name of the namespace in which to generate the type.
        /// </param>
        /// <param name="typeName">
        /// The unqualified name of the type to generate.
        /// </param>
        /// <param name="copyrightNotice">
        /// The text of the copyright notice to place at the top of the generated file.
        /// </param>
        /// <param name="description">
        /// The text of the summary comment on the type.
        /// </param>
        public string Generate(string namespaceName, string typeName, string copyrightNotice, string description)
        {
            TypeName = typeName.ToPascalCase();
            TypeDeclaration = GenerateTypeDeclaration();

            AddMembers();

            return TypeDeclaration.Format(copyrightNotice, Usings, namespaceName, description);
        }

        protected void AddUsing(string namespaceName)
        {
            Usings = Usings ?? new List<string>();

            Usings.Add(namespaceName);
        }
    }
}
