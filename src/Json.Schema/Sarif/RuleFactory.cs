// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Json.Schema.Sarif
{
    // TODO Not best name. Maybe RuleDictionary.Instance and have it implement IDictionary.
    internal static class RuleFactory
    {
        // TODO: Make list immutable
        // TODO: Put strings in resources

        private const string DefaultMessageFormatId = "default";
        private const string ErrorCodeFormat = "JS{0:D4}";

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
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidAdditionalPropertiesType),
                DefaultLevel = ResultLevel.Error,
                Name = "InvalidAdditionalPropertiesType",
                FullDescription = "The value of the additionalProperties schema property is neither a boolean nor an object.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorInvalidAdditionalPropertiesType
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidTypeType)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidTypeType),
                DefaultLevel = ResultLevel.Error,
                Name = "InvalidTypeType",
                FullDescription = "The value of the type schema property is neither a string nor an array of strings.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorInvalidTypeType
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidTypeString)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidTypeString),
                DefaultLevel = ResultLevel.Error,
                Name = "InvalidTypeString",
                FullDescription = "The string value of the type schema property is not one of the valid values.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorInvalidTypeString
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.WrongType)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.WrongType),
                DefaultLevel = ResultLevel.Error,
                Name = "WrongType",
                FullDescription = "An instance has a type not permitted by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorWrongType
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.RequiredPropertyMissing)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.RequiredPropertyMissing),
                DefaultLevel = ResultLevel.Error,
                Name = "RequiredPropertyMissing",
                FullDescription = "A property required by the schema is missing.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorRequiredPropertyMissing
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewArrayItems)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewArrayItems),
                DefaultLevel = ResultLevel.Error,
                Name = "TooFewArrayItems",
                FullDescription = "An array has fewer elements than permitted by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorTooFewArrayItems
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooManyArrayItems)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooManyArrayItems),
                DefaultLevel = ResultLevel.Error,
                Name = "TooManyArrayItems",
                FullDescription = "An array has more elements than permitted by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorTooManyArrayItems
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.AdditionalPropertiesProhibited)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.AdditionalPropertiesProhibited),
                DefaultLevel = ResultLevel.Error,
                Name = "AdditionalPropertiesProhibited",
                FullDescription = "A property not defined by the schema is present, and the schema does not permit additional properties.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorAdditionalPropertiesProhibited
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooLarge)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooLarge),
                DefaultLevel = ResultLevel.Error,
                Name = "ValueTooLarge",
                FullDescription = "A numeric value is greater than the maximum value permitted by the schema's 'maximum' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorValueTooLarge
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooLargeExclusive)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooLargeExclusive),
                DefaultLevel = ResultLevel.Error,
                Name = "ValueTooLargeExclusive",
                FullDescription = "A numeric value is greater than or equal to the exclusive maximum value permitted by the schema's 'maximum' and 'exclusiveMaximum' properties.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorValueTooLargeExclusive
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooSmall)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooSmall),
                DefaultLevel = ResultLevel.Error,
                Name = "ValueTooSmall",
                FullDescription = "A numeric value is less than the minimum value permitted by the schema's 'minimum' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorValueTooSmall
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooSmallExclusive)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValueTooSmallExclusive),
                DefaultLevel = ResultLevel.Error,
                Name = "ValueTooSmallExclusive",
                FullDescription = "A numeric value is less than or equal to the exclusive minimum value permitted by the schema's 'minimum' and 'exclusiveMinimum' properties.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorValueTooSmallExclusive
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooManyProperties)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooManyProperties),
                DefaultLevel = ResultLevel.Error,
                Name = "TooManyProperties",
                FullDescription = "An object has more properties than permitted by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorTooManyProperties
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewProperties)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewProperties),
                DefaultLevel = ResultLevel.Error,
                Name = "TooFewProperties",
                FullDescription = "An object has fewer properties than permitted by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorTooFewProperties
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewProperties)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewProperties),
                DefaultLevel = ResultLevel.Error,
                Name = "TooFewProperties",
                FullDescription = "An object has fewer properties than permitted by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorTooFewProperties
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAMultiple)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAMultiple),
                DefaultLevel = ResultLevel.Error,
                Name = "NotAMultiple",
                FullDescription = "A numeric value is not a multiple of the value specified by the schema's 'multipleOf' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorNotAMultiple
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.StringTooLong)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.StringTooLong),
                DefaultLevel = ResultLevel.Error,
                Name = "StringTooLong",
                FullDescription = "A string is longer than permitted by the schema's 'maxLength' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorStringTooLong
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.StringTooShort)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.StringTooShort),
                DefaultLevel = ResultLevel.Error,
                Name = "StringTooShort",
                FullDescription = "A string is shorter than permitted by the schema's 'minLength' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorStringTooShort
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.StringDoesNotMatchPattern)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.StringDoesNotMatchPattern),
                DefaultLevel = ResultLevel.Error,
                Name = "StringDoesNotMatchPattern",
                FullDescription = "A string does not match the regular expression pattern specified by the schema.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorStringDoesNotMatchPattern
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAllOf)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAllOf),
                DefaultLevel = ResultLevel.Error,
                Name = "NotAllOf",
                FullDescription = "An instance does not successfully validate against all of the schemas by the schema's 'allOf' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorNotAllOf
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAnyOf)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotAnyOf),
                DefaultLevel = ResultLevel.Error,
                Name = "NotAnyOf",
                FullDescription = "An instance does not successfully validate against any of the schemas by the schema's 'anyOf' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorNotAnyOf
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotOneOf)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotOneOf),
                DefaultLevel = ResultLevel.Error,
                Name = "NotOneOf",
                FullDescription = "An instance does not successfully validate against exactly one of the schemas by the schema's 'oneOf' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorNotOneOf
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidEnumValue)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.InvalidEnumValue),
                DefaultLevel = ResultLevel.Error,
                Name = "InvalidEnumValue",
                FullDescription = "A string instance does not match any of the values specified by the schema's 'enum' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorInvalidEnumValue
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotUnique)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.NotUnique),
                DefaultLevel = ResultLevel.Error,
                Name = "NotUnique",
                FullDescription = "An array's elements are not unique, as required by the schema's 'uniqueItems' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorNotUnique
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewItemSchemas)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.TooFewItemSchemas),
                DefaultLevel = ResultLevel.Error,
                Name = "TooFewItemSchemas",
                FullDescription = "An array has more elements than the number of elements in the array specified by the schema's 'items' property, and the schema does not permit additional array items.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorTooFewItemSchemas
                }
            },

            [ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValidatesAgainstNotSchema)] =
            new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(ErrorNumber.ValidatesAgainstNotSchema),
                DefaultLevel = ResultLevel.Error,
                Name = "ValidatesAgainstNotSchema",
                FullDescription = "An instance successfully validates against the schema specified by the schema's 'not' property.",
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = Resources.ErrorValidatesAgainstNotSchema
                }
            }
        };

        internal static Rule GetRuleFromRuleId(string ruleId)
        {
            return s_ruleDictionary[ruleId];
        }

        internal static Rule GetRuleFromErrorNumber(ErrorNumber errorNumber)
        {
            string ruleId = string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);

            return GetRuleFromRuleId(ruleId);
        }
    }
}
