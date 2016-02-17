// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.JSchema
{
    public class JSchemaException : Exception
    {
        public static JSchemaException Create(string messageFormat, params object[] messageArgs)
        {
            return new JSchemaException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    messageFormat,
                    messageArgs));
        }

        public JSchemaException()
        {
        }

        public JSchemaException(string message)
            : base(message)
        {
        }

        public JSchemaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
