// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.Json.Schema
{
    /// <summary>
    /// Error numbers from reading a JSON schema.
    /// </summary>
    /// <remarks>
    /// Do not alter or reuse the integer values used in the enum initializers.
    /// They must remain stable because the error codes will be documented.
    /// </remarks>
    public enum ErrorNumber
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        #region Errors in schema document

        /// <summary>
        /// The schema is not a valid JSON document.
        /// </summary>
        SyntaxError = 1,

        /// <summary>
        /// In the schema, a property that is required to be a string is not a string.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "title": 2
        /// }
        /// </code>
        /// </example>
        NotAString = 2,

        /// <summary>
        /// In the schema, the <code>additionalProperties</code> property is neither a Boolean
        /// nor an object.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "additionalProperties": 2
        /// }
        /// </code>
        /// </example>
        InvalidAdditionalPropertiesType = 3,

        /// <summary>
        /// In the schema, the <code>items</code> property is neither an object nor an array.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "items": 2
        /// }
        /// </code>
        /// </example>
        InvalidItemsType = 4,

        /// <summary>
        /// In the schema, the <code>type</code> property is neither a string nor an array of strings.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "type": 2
        /// }
        /// </code>
        /// </example>
        InvalidTypeType = 5,

        /// <summary>
        /// In the schema, the <code>type</code> property contains an invalid type string.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "type": [ "string", "invalid" ]
        /// }
        /// </code>
        /// </example>
        InvalidTypeString = 6,

        /// <summary>
        /// In the schema, the <code>additionalItems</code> property is neither a Boolean
        /// nor a schema.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "additionalItems": 2
        /// }
        /// </code>
        /// </example>
        InvalidAdditionalItemsType = 7,

        /// <summary>
        /// In the schema, one of the properties of the <code>dependencies</code> property
        /// is either an object nor an array.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "dependencies": {
        ///     "a": 2
        ///   }
        /// }
        /// </code>
        /// </example>
        InvalidDependencyType = 8,

        /// <summary>
        /// In the schema, one of the properties of the <code>dependencies</code> property
        /// is an array which contains an element which is not a string.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///   "dependencies": {
        ///     "a": [ "good", 42 ]
        ///   }
        /// }
        /// </code>
        /// </example>
        InvalidPropertyDependencyType = 9,

        #endregion Errors in schema document

        #region Errors in instance document

        /// <summary>
        /// A token has the wrong type.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer"
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "x"
        /// </code>
        /// </example>
        WrongType = 1001,

        /// <summary>
        /// A required property is missing.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "required": [ "a", "b", "c" ]
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// {
        ///   "a": 1,
        ///   "b": 2
        /// }
        /// </code>
        /// </example>
        RequiredPropertyMissing = 1002,

        /// <summary>
        /// An array has too few items.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "array",
        ///   "minItems": 3
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// [ 1, 2 ]
        /// </code>
        /// </example>
        TooFewArrayItems = 1003,

        /// <summary>
        /// An array has too many items.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "array",
        ///   "minItems": 3
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// [ 1, 2, 3, 4 ]
        /// </code>
        /// </example>
        TooManyArrayItems = 1004,

        /// <summary>
        /// An object has a property not permitted by the schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// {
        ///   "a": 3
        /// }
        /// </code>
        /// </example>
        AdditionalPropertiesProhibited = 1005,

        /// <summary>
        /// A numeric instance has a value greater than the maximum value permitted by the
        /// schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer",
        ///   "maximum": 2
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "3"
        /// </code>
        /// </example>
        ValueTooLarge = 1006,

        /// <summary>
        /// A numeric instance has a value greater than or equal to the exclusive maximum
        /// value permitted by the schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer",
        ///   "maximum": 2,
        ///   "exclusiveMaximum": true
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "2"
        /// </code>
        /// </example>
        ValueTooLargeExclusive = 1007,

        /// <summary>
        /// A numeric instance has a value less than the minimum value permitted by the
        /// schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer",
        ///   "minimum": 2
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "1"
        /// </code>
        /// </example>
        ValueTooSmall = 1008,

        /// <summary>
        /// A numeric instance has a value less than or equal to the exclusive minimum
        /// value permitted by the schema.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "integer",
        ///   "minimum": 2,
        ///   "exclusiveMinimum": true
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "2"
        /// </code>
        /// </example>
        ValueTooSmallExclusive = 1009,

        /// <summary>
        /// An object instance has more properties than the schema permits.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "object",
        ///   "maxProperties": 2,
        ///   "additionalProperties": true
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// {
        ///   "a": 1,
        ///   "b": 2,
        ///   "c": 3
        /// }
        /// </code>
        /// </example>
        TooManyProperties = 1010,

        /// <summary>
        /// An object instance has fewer properties than the schema permits.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "object",
        ///   "minProperties": 2,
        ///   "additionalProperties": true
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// {
        ///   "a": 1
        /// }
        /// </code>
        /// </example>
        TooFewProperties = 1011,


        /// <summary>
        /// An numeric instance is not a multiple of the specified value.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "number",
        ///   "multipleOf": 2
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "4.001"
        /// </code>
        /// </example>
        NotAMultiple = 1012,

        /// <summary>
        /// A string instance is longer than the schema permits.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "string",
        ///   "maxLength": 2
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "abc"
        /// </code>
        /// </example>
        StringTooLong = 1013,

        /// <summary>
        /// A string instance is shorter than the schema permits.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "string",
        ///   "minLength": 2
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "a"
        /// </code>
        /// </example>
        StringTooShort = 1014,

        /// <summary>
        /// A string instance does not match the required regular expression.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "string",
        ///   "pattern": "\\d{3}"
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "a12b"
        /// </code>
        /// </example>
        StringDoesNotMatchPattern = 1015,

        /// <summary>
        /// An instance does not validate successfully against all of the schemas specified by "allOf".
        /// </summary>
        NotAllOf = 1016,

        /// <summary>
        /// An instance does not validate successfully against any of the schemas specified by "anyOf".
        /// </summary>
        NotAnyOf = 1017,

        /// <summary>
        /// An instance validates successfully against either zero or more than one of the schemas
        /// specified by "oneOf".
        /// </summary>
        NotOneOf = 1018,

        /// <summary>
        /// An instance does not match any of the values specified by "enum".
        /// </summary>
        InvalidEnumValue = 1019,

        /// <summary>
        /// The elements of an array instance are not unique.
        /// </summary>
        NotUnique = 1020,

        /// <summary>
        /// The array has more items than does the array of schemas specified by "items",
        /// and "additionalItems" is not specified to allow additional array items.
        /// </summary>
        TooFewItemSchemas = 1021,

        /// <summary>
        /// An instance validates successfully agains the schema specified by "not".
        /// </summary>
        ValidatesAgainstNotSchema = 1022,

        /// <summary>
        /// The instance contains a property specified by "dependencies", but it does
        /// not contain all the properties specified by the corresponding property dependency.
        /// </summary>
        DependentPropertyMissing = 1023,


        /// <summary>
        /// A string instance does not match the required format.
        /// </summary>
        /// <example>
        /// Schema:
        /// <code>
        /// {
        ///   "type": "string",
        ///   "format": "date-time"
        /// }
        /// </code>
        /// 
        /// Instance:
        /// <code>
        /// "2023-02-03:T12:00:00Z"
        /// </code>
        /// </example>
        StringDoesNotMatchFormat = 1024,

        #endregion Errors in instance document
    }
}
