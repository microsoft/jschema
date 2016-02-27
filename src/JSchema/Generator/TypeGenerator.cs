// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.JSchema.Generator
{
    public abstract class TypeGenerator
    {
        protected string _text;

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
            if (_text == null)
            {
                throw new InvalidOperationException(Resources.ErrorTextNotYetGenerated);
            }

            return _text;
        }
    }
}
