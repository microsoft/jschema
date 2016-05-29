// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a code generation hint that tells the generator to add
    /// an attribute to a property.
    /// </summary>
    public class AttributeHint : CodeGenHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeHint"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The fully qualified type name of the attribute to generate, without
        /// the "Attribute" suffix.
        /// </param>
        /// <param name="namespaceName">
        /// The namespace in which the type specified by <paramref name="typeName"/>
        /// resides. Can be <code>null</code> if <code>typeName</code> specifies a
        /// fully qualified type name.
        /// </param>
        /// <param name="arguments">
        /// Arguments to the attribute's constructor.
        /// </param>
        /// <param name="properties">
        /// The names and values of properties to set on the attribute.
        /// </param>
        public AttributeHint(
            string typeName,
            string namespaceName,
            IEnumerable<string> arguments,
            IDictionary<string, string> properties)
        {
            TypeName = typeName;
            NamespaceName = namespaceName;
            Arguments = arguments != null ? arguments.ToList() : new List<string>();
            Properties = properties != null ? properties : new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the fully qualified type name of the attribute to generate, without
        /// the "Attribute" suffix.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the namespace in which the type specified by <paramref name="typeName"/>
        /// resides. Can be <code>null</code> if <code>typeName</code> specifies a
        /// fully qualified type name.
        /// </summary>
        public string NamespaceName { get; }

        /// <summary>
        /// Gets the arguments to the attribute's constructor.
        /// </summary>
        public IList<string> Arguments { get; }

        /// <summary>
        /// Gets the names and values of the properties to set on the attribute
        /// </summary>
        public IDictionary<string, string> Properties { get; }
    }
}
