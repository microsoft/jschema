// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Contains information about a property inferred from the schema.
    /// </summary>
    public class PropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfo"/> class.
        /// </summary>
        /// <param name="description">
        /// The description of the property, for use in a summary comment.
        /// </param>
        /// <param name="serializedName">
        /// The name of the property as serialized in the JSON file.
        /// </param>
        /// <param name="comparisonKind">
        /// The kind of comparison code required by the property.
        /// </param>
        /// <param name="hashKind">
        /// The kind of hash value computation code required by the property.
        /// </param>
        /// <param name="initializationKind">
        /// The kind of initialization code required by the property.
        /// </param>
        /// <param name="type">
        /// The type of the property.
        /// </param>
        /// <param name="typeName">
        /// The name of the type of the property.
        /// </param>
        /// <param name="namespaceName">
        /// The qualified name of the namespace declaration required by this type,
        /// or <code>null</code> if no namespace declaration is required.
        /// </param>
        /// <param name="isRequired">
        /// <code>true</code> if this property is required by the schema;
        /// otherwise <code>false</code>.
        /// </param>
        /// <param name="defaultValue">
        /// The default value, if any, specified by the schema; otherwise <code>null</code>.
        /// </param>
        /// <param name="isOfSchemaDefinedType">
        /// <code>true</code> if this property is of a type defined by the schema (or an;
        /// array of a schema-defined type otherwise <code>false</code>.
        /// </param>
        /// <param name="arrayRank">
        /// The array rank of the property type. 0 means the property is not an array.
        /// </param>
        /// <param name="declarationOrder">
        /// The 0-based order in which the property was declared in the schema.
        /// </param>
        public PropertyInfo(
            string description,
            string serializedName,
            ComparisonKind comparisonKind,
            HashKind hashKind,
            InitializationKind initializationKind,
            TypeSyntax type,
            string namespaceName,
            bool isRequired,
            object defaultValue,
            bool isOfSchemaDefinedType,
            int arrayRank,
            int declarationOrder)
        {
            Description = description;
            SerializedName = serializedName;
            ComparisonKind = comparisonKind;
            HashKind = hashKind;
            InitializationKind = initializationKind;
            Type = type;
            TypeName = type.ToString();
            NamespaceName = namespaceName;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            IsOfSchemaDefinedType = isOfSchemaDefinedType;
            ArrayRank = arrayRank;
            DeclarationOrder = declarationOrder;
        }

        /// <summary>
        /// Gets a description for the property, for use in a summary comment.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the name of the property as serialized in the JSON file.
        /// </summary>
        public string SerializedName { get; }

        /// <summary>
        /// Gets a value that specifies the kind of comparison code that must be
        /// generated for the property in the implementation of the
        /// <see cref="IEquatable{T}.Equals()" /> method.
        /// </summary>
        public ComparisonKind ComparisonKind { get; }

        /// <summary>
        /// Gets a value that specifies the kind of code that must be generated to
        /// compute the hash for the property in the implementation of the
        /// <see cref="Object.GetHashCode()" /> method.
        /// </summary>
        public HashKind HashKind { get; }

        /// <summary>
        /// Gets a value that specifies the kind of initialization code that must be
        /// generated for the property in the implementation of the <code>Init</code>
        /// method.
        /// </summary>
        public InitializationKind InitializationKind { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public TypeSyntax Type { get; }

        /// <summary>
        /// Gets the name of the type of the property.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the qualified name of the namespace declaration required by this type,
        /// or <code>null</code> if no namespace declaration is required.
        /// </summary>
        public string NamespaceName { get; }

        /// <summary>
        /// Gets a value indicating whether this property is required by the schema.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Gets this property's default value, if the schema specifies one; otherwise <code>null</code>.
        /// </summary>
        public object DefaultValue;

        /// <summary>
        /// Gets a value indicating whether this property is of a type defined by the schema.
        /// </summary>
        public bool IsOfSchemaDefinedType { get; }

        /// <summary>
        /// Gets the array rank of the property type. 0 means the property is not an array.
        /// </summary>
        public int ArrayRank { get; }

        /// <summary>
        /// Gets the 0-based order in which the property was declared in the schema.
        /// </summary>
        public int DeclarationOrder { get; }
    }
}
