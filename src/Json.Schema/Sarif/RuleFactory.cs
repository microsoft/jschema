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

        internal const string DefaultMessageFormatId = "default";
        private const string ErrorCodeFormat = "JS{0:D4}";

        private static Rule MakeRule(ErrorNumber errorNumber, string fullDescription, string messageFormat)
        {
            string messageFormatWithPath = string.Format(CultureInfo.CurrentCulture, Resources.ErrorMessageFormatWithPath, messageFormat);

            return new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(errorNumber),
                DefaultLevel = ResultLevel.Error,
                Name = errorNumber.ToString(),
                FullDescription = fullDescription,
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = messageFormatWithPath
                }
            };
        }

        private static readonly Dictionary<ErrorNumber, Rule> s_ruleDictionary = new Dictionary<ErrorNumber, Rule>
        {
            [ErrorNumber.NotAString] = MakeRule(
                ErrorNumber.NotAString,
                Resources.RuleDescriptionNotAString,
                Resources.ErrorNotAString),

            [ErrorNumber.InvalidAdditionalPropertiesType] = MakeRule(
                ErrorNumber.InvalidAdditionalPropertiesType,
                Resources.RuleDescriptionInvalidAdditionalPropertiesType,
                Resources.ErrorInvalidAdditionalPropertiesType),

            [ErrorNumber.InvalidTypeType] = MakeRule(
                ErrorNumber.InvalidTypeType,
                Resources.RuleDescriptionInvalidTypeType,
                Resources.ErrorInvalidTypeType),

            [ErrorNumber.InvalidTypeString] = MakeRule(
                ErrorNumber.InvalidTypeString,
                Resources.RuleDescriptionInvalidTypeString,
                Resources.ErrorInvalidTypeString),

            [ErrorNumber.WrongType] = MakeRule(
                ErrorNumber.WrongType,
                Resources.RuleDescriptionWrongType,
                Resources.ErrorWrongType),

            [ErrorNumber.RequiredPropertyMissing] = MakeRule(
                ErrorNumber.RequiredPropertyMissing,
                Resources.RuleDescriptionRequiredPropertyMissing,
                Resources.ErrorRequiredPropertyMissing),

            [ErrorNumber.TooFewArrayItems] = MakeRule(
                ErrorNumber.TooFewArrayItems,
                Resources.RuleDescriptionTooFewArrayItems,
                Resources.ErrorTooFewArrayItems),

            [ErrorNumber.TooManyArrayItems] = MakeRule(
                ErrorNumber.TooManyArrayItems,
                Resources.RuleDescriptionTooManyArrayItems,
                Resources.ErrorTooManyArrayItems),

            [ErrorNumber.AdditionalPropertiesProhibited] = MakeRule(
                ErrorNumber.AdditionalPropertiesProhibited,
                Resources.RuleDescriptionAdditionalPropertiesProhibited,
                Resources.ErrorAdditionalPropertiesProhibited),

            [ErrorNumber.ValueTooLarge] = MakeRule(
                ErrorNumber.ValueTooLarge,
                Resources.RuleDescriptionValueTooLarge,
                Resources.ErrorValueTooLarge),

            [ErrorNumber.ValueTooLargeExclusive] = MakeRule(
                ErrorNumber.ValueTooLargeExclusive,
                Resources.RuleDescriptionValueTooLargeExclusive,
                Resources.ErrorValueTooLargeExclusive),

            [ErrorNumber.ValueTooSmall] = MakeRule(
                ErrorNumber.ValueTooSmall,
                Resources.RuleDescriptionValueTooSmall,
                Resources.ErrorValueTooSmall),

            [ErrorNumber.ValueTooSmallExclusive] = MakeRule(
                ErrorNumber.ValueTooSmallExclusive,
                Resources.RuleDescriptionValueTooSmallExclusive,
                Resources.ErrorValueTooSmallExclusive),

            [ErrorNumber.TooManyProperties] = MakeRule(
                ErrorNumber.TooManyProperties,
                Resources.RuleDescriptionTooManyProperties,
                Resources.ErrorTooManyProperties),

            [ErrorNumber.TooFewProperties] = MakeRule(
                ErrorNumber.TooFewProperties,
                Resources.RuleDescriptionTooFewProperties,
                Resources.ErrorTooFewProperties),

            [ErrorNumber.NotAMultiple] = MakeRule(
                ErrorNumber.NotAMultiple,
                Resources.RuleDescriptionNotAMultiple,
                Resources.ErrorNotAMultiple),

            [ErrorNumber.StringTooLong] = MakeRule(
                ErrorNumber.StringTooLong,
                Resources.RuleDescriptionStringTooLong,
                Resources.ErrorStringTooLong),

            [ErrorNumber.StringTooShort] = MakeRule(
                ErrorNumber.StringTooShort,
                Resources.RuleDescriptionStringTooShort,
                Resources.ErrorStringTooShort),

            [ErrorNumber.StringDoesNotMatchPattern] = MakeRule(
                ErrorNumber.StringDoesNotMatchPattern,
                Resources.RuleDescriptionStringDoesNotMatchPattern,
                Resources.ErrorStringDoesNotMatchPattern),

            [ErrorNumber.NotAllOf] = MakeRule(
                ErrorNumber.NotAllOf,
                Resources.RuleDescriptionNotAllOf,
                Resources.ErrorNotAllOf),

            [ErrorNumber.NotAnyOf] = MakeRule(
                ErrorNumber.NotAnyOf,
                Resources.RuleDescriptionNotAnyOf,
                Resources.ErrorNotAnyOf),

            [ErrorNumber.NotOneOf] = MakeRule(
                ErrorNumber.NotOneOf,
                Resources.RuleDescriptionNotOneOf,
                Resources.ErrorNotOneOf),

            [ErrorNumber.InvalidEnumValue] = MakeRule(
                ErrorNumber.InvalidEnumValue,
                Resources.RuleDescriptionInvalidEnumValue,
                Resources.ErrorInvalidEnumValue),

            [ErrorNumber.NotUnique] = MakeRule(
                ErrorNumber.NotUnique,
                Resources.RuleDescriptionNotUnique,
                Resources.ErrorNotUnique),

            [ErrorNumber.TooFewItemSchemas] = MakeRule(
                ErrorNumber.TooFewItemSchemas,
                Resources.RuleDescriptionTooFewItemSchemas,
                Resources.ErrorTooFewItemSchemas),

            [ErrorNumber.ValidatesAgainstNotSchema] = MakeRule(
                ErrorNumber.ValidatesAgainstNotSchema,
                Resources.RuleDescriptionValidatesAgainstNotSchema,
                Resources.ErrorValidatesAgainstNotSchema)
        };

        internal static Rule GetRuleFromRuleId(string ruleId)
        {
            var errorNumber = (ErrorNumber)int.Parse(ruleId.Substring(2));
            return GetRuleFromErrorNumber(errorNumber);
        }

        internal static Rule GetRuleFromErrorNumber(ErrorNumber errorNumber)
        {
            return s_ruleDictionary[errorNumber];
        }
    }
}
