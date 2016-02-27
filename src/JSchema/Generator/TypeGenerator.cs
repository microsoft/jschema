// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.JSchema.Generator
{
    public abstract class TypeGenerator
    {
        protected HashSet<string> Usings { get; private set; }

        protected string Text { get; set; }

        protected void AddUsing(string namespaceName)
        {
            if (Usings == null)
            {
                Usings = new HashSet<string>();
            }

            Usings.Add(namespaceName);
        }

        /// <summary>
        /// Gets the text of the generated class.
        /// </summary>
        /// <returns>
        /// A string containing the text of the generated class.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If <see cref="Finish"/> has not yet been called.
        /// </exception>
        public string GetText()
        {
            if (Text == null)
            {
                throw new InvalidOperationException(Resources.ErrorTextNotYetGenerated);
            }

            return Text;
        }
    }
}
