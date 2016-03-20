// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using Microsoft.JSchema;

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
        /// Gets or sets the name of the namespace in which the classes will be generated.
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the class at the root of the generated object model
        /// </summary>
        public string RootClassName { get; set; }

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
        /// Gets or sets a value indicating whether method overrides such as
        /// <code>Equals</code> and <code>GetHashCode</code> are to be generated.
        /// </summary>
        public bool GenerateOverrides { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the code necessary to clone
        /// instances of the classes in the object model is to be generated.
        /// </summary>
        public bool GenerateCloningCode { get; set; }

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
                throw JSchemaException.Create(sb.ToString());
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
