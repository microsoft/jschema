// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema.Sarif;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Exception thrown when a JSON syntax error is encountered.
    /// </summary>
    public class JsonSyntaxException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSyntaxException"/> class.
        /// </summary>
        public JsonSyntaxException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSyntaxException"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        public JsonSyntaxException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSyntaxException"/> class
        /// with the specified message and inner exception.
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        /// <param name="innerException">
        /// An exception that was the cause of this exception.
        /// </param>
        public JsonSyntaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSyntaxException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> object that holds the serialized object data
        /// for the exception being thrown.
        /// </param>
        /// The <see cref="StreamingContext"/> object that contains contextual information
        /// about the source or destination.
        public JsonSyntaxException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSyntaxException"/> class
        /// with a file name and with information from a <see cref="JsonReaderException"/>.
        /// </summary>
        public JsonSyntaxException(string fileName, JsonReaderException ex)
        {
            Rule rule = RuleFactory.GetRuleFromErrorNumber(ErrorNumber.SyntaxError);

            Result = new Result
            {
                RuleId = rule.Id,
                Level = rule.DefaultLevel,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri(fileName, UriKind.RelativeOrAbsolute),
                            Region = new Region
                            {
                                StartLine = ex.LineNumber,
                                StartColumn = ex.LinePosition
                            }
                        }
                    }
                },

                FormattedRuleMessage = new FormattedRuleMessage
                {
                    FormatId = RuleFactory.DefaultMessageFormatId,
                    Arguments = new List<string>
                    {
                        ex.Path,
                        ex.Message
                    }
                }
            };
        }

        public Result Result { get; }
    }
}
