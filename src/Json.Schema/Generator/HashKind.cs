// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema.Generator
{
    /// <summary>
    /// Values that specify the type of code that must be generated to compute the
    /// hash for each property in the implementation of the
    /// <see cref="Object.GetHashCode" /> method.
    /// </summary>
    public enum HashKind
    {
        /// <summary>
        /// Do not generate code to compute the hash for this property.
        /// </summary>
        None = 0,

        /// <summary>
        /// Generate code to compute the hash for a single object of a value type such as
        /// <code>int</code>, without first checking for <code>null</code>.
        /// </summary>
        /// <example>
        /// The class generator generates code like this for a scalar value type:
        /// <code>
        /// result = (result * 31) + IntProp.GetHashCode();
        /// </code>
        /// </example>
        ScalarValueType,

        /// <summary>
        /// Generates code to compute the hash for a single object of a reference type 
        /// such as <code>string</code>, after first checking for <code>null</code>.
        /// </summary>
        /// <example>
        /// The class generator generates code like this for a scalar reference type:
        /// <code>
        /// if (StringProp != null)
        /// {
        ///     result = (result * 31) + StringProp.GetHashCode();
        /// }
        /// </code>
        /// </example>
        ScalarReferenceType,

        /// <summary>
        /// Generates code to compute the hash for a collection, after first checking for
        /// <code>null</code>.
        /// </summary>
        /// <remarks>
        /// The generated code depends on whether the collection elements are of
        /// reference type, value type, or collection type. At present the class
        /// generator does not support collections of dictionaries.
        /// </remarks>
        /// <example>
        /// The class generator generates code like this for a collection of objects of
        /// scalar reference type, such as <code>string</code>:
        /// <code>
        /// if (ArrayProp != null)
        /// {
        ///     result = *= 31;
        ///     for (int value_0; value_0 < ArrayProp.Count; ++value_0)
        ///     {
        ///         var value_1 = ArrayProp[value_0];
        ///         if (value_1 != null)
        ///         {
        ///             result += value_1.GetHashCode();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        Collection,

        /// <summary>
        /// Generates code to compute the hash for a dictionary, after first checking for
        /// <code>null</code>.
        /// </summary>
        /// <remarks>
        /// At present the class generator only supports dictionaries from
        /// <code>string</code> to <code>string</code>.
        /// </remarks>
        /// <example>
        /// The class generator generates code like this for a dictionary from
        /// <code>string</code> to <code>string</code>.
        /// <code>
        /// if (DictProp != null)
        /// {
        ///     result *= 31;
        ///     TODO: Look at Sarif.cs and fill in the rest here.
        /// }
        /// <code>
        /// </example>
        Dictionary
    }
}
