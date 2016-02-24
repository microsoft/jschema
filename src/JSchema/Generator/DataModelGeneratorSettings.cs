// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.JSchema.Generator
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
        public string CopyrightFilePath { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that maps from the URI of a schema to a set of
        /// hints that control code generation for the type generated from that schema.
        /// </summary>
        public Dictionary<UriOrFragment, CodeGenHint[]> HintDictionary { get; set; }

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
                throw new JSchemaException(sb.ToString());
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
