// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using Microsoft.Json.Schema.ToDotNet.Hints;

namespace Microsoft.Json.Schema.ToDotNet
{
    /// <summary>
    /// Settings that control the operation of the <see cref="DataModelGenerator"/>.
    /// </summary>
    public class DataModelGeneratorSettings
    {
        /// <summary>
        /// Gets or sets the path to the directory in which the classes will be generated.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether files in the specified output directory
        /// should be overwritten.
        /// </summary>
        public bool ForceOverwrite { get; set; }

        /// <summary>
        /// Gets or sets a string that is used as a suffix to every generated type name.
        /// </summary>
        public string TypeNameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the name of the namespace in which the classes will be generated.
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// Gets the name of the namespace in which the classes will be generated, including
        /// any specified suffix.
        /// </summary>
        public string SuffixedNamespaceName => Utilities.QualifyNameWithSuffix(NamespaceName, TypeNameSuffix);

        /// <summary>
        /// Gets or sets the name of the class at the root of the generated object model
        /// </summary>
        public string RootClassName { get; set; }

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        /// <remarks>
        /// This name is used as a prefix for some of the generated type names.
        /// </remarks>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the path of the file containing the copyright notice to place
        /// at the top of each file.
        /// </summary>
        public string CopyrightNotice { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that maps from the URI of a schema to a set of
        /// hints that control code generation for the type generated from that schema.
        /// </summary>
        public HintDictionary HintDictionary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether implementations of
        /// <see cref="System.Collections.Generic.IEqualityComparer{T}"/> are to be generated.
        /// </summary>
        public bool GenerateEqualityComparers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the code necessary to clone
        /// instances of the classes in the object model is to be generated.
        /// </summary>
        public bool GenerateCloningCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generated classes should
        /// be declared <code>sealed</code>. If <code>false</code>, the generated
        /// classes are declared <code>partial</code>.
        /// </summary>
        public bool SealClasses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the members of the generated
        /// classes whould be declared <code>virtual</code>.
        /// </summary>
        public bool VirtualMembers { get; set; }

        internal void Validate()
        {
            var sb = new StringBuilder();

            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                ReportMissingProperty(nameof(OutputDirectory), sb);
            }

            if (string.IsNullOrWhiteSpace(NamespaceName))
            {
                ReportMissingProperty(nameof(NamespaceName), sb);
            }

            if (string.IsNullOrWhiteSpace(NamespaceName))
            {
                ReportMissingProperty(nameof(RootClassName), sb);
            }

            if (sb.Length > 0)
            {
                throw Error.CreateException(sb.ToString());
            }
        }

        private void ReportMissingProperty(string propertyName, StringBuilder sb)
        {
            sb.AppendLine(string.Format(
                CultureInfo.CurrentCulture,
                Resources.ErrorSettingsPropertyMissing,
                propertyName,
                nameof(DataModelGeneratorSettings)));
        }
    }
}
