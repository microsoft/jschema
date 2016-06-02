// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Describes that conditions that must be satisfied when an object contains
    /// a property with the specified name.
    /// </summary>
    /// <remarks>
    /// See JSON Schema v4, Sec. 5.4.5.
    /// </remarks>
    public class Dependency: IEquatable<Dependency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dependency"/> class from the
        /// specified schema dependency.
        /// </summary>
        /// <param name="schemaDependency">
        /// The schema against which an instance must validate successfully if it
        /// has a property of the name associated with this dependency.
        /// </param>
        public Dependency(JsonSchema schemaDependency)
        {
            SchemaDependency = schemaDependency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dependency"/> class from the
        /// specified property dependencies.
        /// </summary>
        /// <param name="propertyDependencies">
        /// The set of property names which an instance must also have if it has a
        /// property of the name associated with this dependency.
        /// </param>
        public Dependency(IList<string> propertyDependencies)
        {
            PropertyDependencies = propertyDependencies.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dependency"/> class from an
        /// existing instance.
        /// </summary>
        /// <param name="other">
        /// The existing instance from which this instance is to be initialized.
        /// </param>
        public Dependency(Dependency other)
        {
            SchemaDependency = other.SchemaDependency;
            PropertyDependencies = other.PropertyDependencies;
        }

        /// <summary>
        /// Gets the schema against which an instance must validate successfully if it
        /// has a property of the name associated with this dependency.
        /// </summary>
        /// <remarks>
        /// See JSON Schema v4, Sec. 5.4.5.2.1.
        /// </remarks>
        public JsonSchema SchemaDependency { get; }

        /// <summary>
        /// Gets the set of property names which an instance must also have if it has a
        /// property of the name associated with this dependency.
        /// </summary>
        /// <remarks>
        /// See JSON Schema v4, Sec. 5.4.5.2.2.
        /// </remarks>
        public List<string> PropertyDependencies { get; }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as Dependency);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(new object[] { SchemaDependency, PropertyDependencies });
        }

        #endregion

        #region IEquatable<T>

        public bool Equals(Dependency other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (SchemaDependency != null)
            {
                return SchemaDependency.Equals(other.SchemaDependency);
            }

            if (PropertyDependencies != null)
            {
                return PropertyDependencies.HasSameElementsAs(other.PropertyDependencies);
            }

            return false;
        }

        #endregion

        public static bool operator ==(Dependency left, Dependency right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((object)left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Dependency left, Dependency right)
        {
            return !(left == right);
        }
    }
}
