// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Thrown when the <see cref="SchemaReader"/> encounters an error in the schema.
    /// </summary>
    public class InvalidSchemaException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class.
        /// </summary>
        public InvalidSchemaException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        public InvalidSchemaException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with the specified message and inner exception.
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        /// <param name="innerException">
        /// An exception that was the cause of this exception.
        /// </param>
        public InvalidSchemaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> object that holds the serialized object data
        /// for the exception being thrown.
        /// </param>
        /// The <see cref="StreamingContext"/> object that contains contextual information
        /// about the source or destination.
        public InvalidSchemaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with information describing an error encountered while reading a JSON schema.
        /// </summary>
        /// <param name="jToken">
        /// The token on which the error was encountered.
        /// </param>
        /// <param name="errorNumber">
        /// The error number.
        /// </param>
        /// <param name="args">
        /// A set of values which are to be formatted along with the error number to construct
        /// an error message.
        /// </param>
        public InvalidSchemaException(JToken jToken, ErrorNumber errorNumber, params object[] args)
            : this(new Error(jToken, errorNumber, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with an error encountered while reading a JSON schema.
        /// </summary>
        /// <param name="error">
        /// An error encountered while reading a JSON schema.
        /// </param>
        public InvalidSchemaException(Error error)
            : this(new[] { error })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with a list of errors encountered while reading a JSON schema.
        /// </summary>
        /// <param name="errors">
        /// The list of errors encountered while reading a JSON schema.
        /// </param>
        public InvalidSchemaException(IEnumerable<Error> errors)
            : base(FormatMessage(errors))
        {
            Errors = errors.ToList();
        }

        /// <summary>
        /// Gets the list of errors encountered while reading a JSON schema.
        /// </summary>
        public List<Error> Errors { get; }

        private static string FormatMessage(IEnumerable<Error> errors)
        {
            return string.Join("\n", errors.Select(error => error.Message));
        }
    }
}
