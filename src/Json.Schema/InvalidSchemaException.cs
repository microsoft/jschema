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
            : this(ResultFactory.CreateResult(jToken, errorNumber, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSchemaException"/> class
        /// with an error encountered while reading a JSON schema.
        /// </summary>
        /// <param name="error">
        /// An error encountered while reading a JSON schema.
        /// </param>
        public InvalidSchemaException(Result error)
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
        public InvalidSchemaException(IEnumerable<Result> errors)
            : base(FormatMessage(errors))
        {
            Results = errors.ToList();
        }

        /// <summary>
        /// Gets the list of errors encountered while reading a JSON schema.
        /// </summary>
        public List<Result> Results { get; }

        private static string FormatMessage(IEnumerable<Result> errors)
        {
            return string.Join("\n", errors.Select(e => e.FormatForVisualStudio(s_ruleDictionary[e.RuleId])));
        }

        // TODO: Make list immutable
        // TODO: Put strings in resources

        private const string DefaultMessageFormatId = "default";

        private static readonly Dictionary<string, Rule> s_ruleDictionary = new Dictionary<string, Rule>
        {
            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAString)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAString),
                DefaultLevel = ResultLevel.Error,
                Name = "NotAString",
                FullDescription = "A schema property that is required to be a string is not a string.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorNotAString
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidAdditionalPropertiesType)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAString),
                DefaultLevel = ResultLevel.Error,
                Name = "InvalidAdditionalPropertiesType",
                FullDescription = "The value of the additionalProperties schema property is neither a boolean nor an object.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorInvalidAdditionalProperties
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidTypeType)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidTypeType),
                DefaultLevel = ResultLevel.Error,
                Name = "InvalidTypeType",
                FullDescription = "The value of the type schema property is neither an object nor an array of objects.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorInvalidAdditionalProperties
                }
            },

        };
    }
}
