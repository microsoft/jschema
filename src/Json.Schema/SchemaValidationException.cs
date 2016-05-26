// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema.Sarif;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Thrown when an error occurs in the course of validating an instance against a schema.
    /// </summary>
    public class SchemaValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class.
        /// </summary>
        public SchemaValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        public SchemaValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class
        /// with the specified message and inner exception.
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        /// <param name="innerException">
        /// An exception that was the cause of this exception.
        /// </param>
        public SchemaValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> object that holds the serialized object data
        /// for the exception being thrown.
        /// </param>
        /// The <see cref="StreamingContext"/> object that contains contextual information
        /// about the source or destination.
        public SchemaValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class
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
        public SchemaValidationException(JToken jToken, ErrorNumber errorNumber, params object[] args)
            : this(ResultFactory.CreateResult(jToken, errorNumber, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class
        /// with an error encountered while reading a JSON schema.
        /// </summary>
        /// <param name="result">
        /// An error encountered while reading a JSON schema.
        /// </param>
        public SchemaValidationException(Result result)
            : this(new[] { result })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationException"/> class
        /// with a list of errors encountered while reading a JSON schema.
        /// </summary>
        /// <param name="results">
        /// The list of errors encountered while reading a JSON schema.
        /// </param>
        public SchemaValidationException(IEnumerable<Result> results)
            : base(FormatMessage(results))
        {
            Results = results.ToList();
        }

        /// <summary>
        /// Gets the list of errors encountered while reading a JSON schema.
        /// </summary>
        public List<Result> Results { get; }

        private static string FormatMessage(IEnumerable<Result> results)
        {
            return string.Join(
                "\n",
                results.Select(
                    r => r.FormatForVisualStudio(RuleFactory.GetRuleFromRuleId(r.RuleId))));
        }
    }
}
