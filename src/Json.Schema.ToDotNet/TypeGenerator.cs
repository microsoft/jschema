// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    public abstract class TypeGenerator
    {
        private string _namespaceName;
        private string _copyrightNotice;
        private string _description;

        private const string GeneratedCodeAttributeName = "GeneratedCode";
        private static readonly string s_assemblyName = Assembly.GetCallingAssembly().GetName().Name;
        private static readonly string s_assemblyVersion = Assembly.GetCallingAssembly().GetName().Version.ToString();

        protected TypeGenerator(HintDictionary hintDictionary)
        {
            HintDictionary = hintDictionary;
        }

        protected string TypeName { get; private set; }

        protected HintDictionary HintDictionary { get; }

        /// <summary>
        /// Gets or sets the type declaration being generated.
        /// </summary>
        protected BaseTypeDeclarationSyntax TypeDeclaration { get; set; }

        protected HashSet<string> Usings { get; private set; }

        /// <summary>
        /// Event raised by a derived class when it discovers that another type must be
        /// generated, in addition to the one it is already generating.
        /// </summary>
        /// <remarks>
        /// For example, when the ClassGenerator encounters a property for which an
        /// <see cref="EnumHint"/> is specified, it raises this event to signal that
        /// an enumerated type must also be generated.
        /// </remarks>
        public event EventHandler<AdditionalTypeRequiredEventArgs> AdditionalTypeRequired;

        public abstract BaseTypeDeclarationSyntax CreateTypeDeclaration(JsonSchema schema);

        protected virtual AttributeSyntax[] CreateTypeAttributes()
        {
            return new AttributeSyntax[]
            {
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName(GeneratedCodeAttributeName),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(
                            new AttributeArgumentSyntax[]
                            {
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(s_assemblyName))),
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(s_assemblyVersion))),
                            })))
            };
        }

        /// <summary>
        /// Adds members to the type as directed by the specified schema.
        /// </summary>
        /// <param name="schema">
        /// The JSON schema that determines which members to add to the type.
        /// </param>
        public abstract void AddMembers(JsonSchema schema);

        /// <summary>
        /// Generate the text for a type from a JSON schema.
        /// </summary>
        /// <param name="schema">
        /// The JSON schema that specifies the type members.
        /// </param>
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
        public string Generate(JsonSchema schema, string namespaceName, string typeName, string copyrightNotice, string description)
        {
            _namespaceName = namespaceName;
            _copyrightNotice = copyrightNotice;
            _description = description;

            AddUsing("System.CodeDom.Compiler");    // For GeneratedCodeAttribute.

            TypeName = typeName.ToPascalCase();
            TypeDeclaration = CreateTypeDeclaration(schema);

            AddMembers(schema);
            return Finish();
        }

        protected void AddUsing(string namespaceName)
        {
            if (Usings == null)
            {
                Usings = new HashSet<string>();
            }

            Usings.Add(namespaceName);
        }

        protected void OnAdditionalType(AdditionalTypeRequiredEventArgs e)
        {
            AdditionalTypeRequired?.Invoke(this, e);
        }

        /// <summary>
        /// Performs all actions necessary to finish generating the type.
        /// </summary>
        /// <returns></returns>
        private string Finish()
        {
            TypeDeclaration = TypeDeclaration
                .WithLeadingTrivia(SyntaxHelper.MakeDocComment(_description));

            return TypeDeclaration.Format(_namespaceName, Usings, _copyrightNotice);
        }
    }
}
