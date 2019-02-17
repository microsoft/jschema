// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Json.Schema.Validation
{
    // TODO Not best name. Maybe RuleDictionary.Instance and have it implement IDictionary.
    public static class RuleFactory
    {
        // TODO: Make list immutable

        public const string DefaultRuleMessageId = "default";
        private const string ErrorCodePrefix = "JSON";
        internal static readonly string ErrorCodeFormat = ErrorCodePrefix + "{0:D4}";

        private static ReportingDescriptor MakeRule(ErrorNumber errorNumber, string fullDescription, string messageFormat)
        {
            string messageStringWithPath = string.Format(CultureInfo.CurrentCulture, RuleResources.ErrorMessageStringWithPath, messageFormat);

            return new ReportingDescriptor
            {
                Id = ResultFactory.RuleIdFromErrorNumber(errorNumber),
                DefaultConfiguration = new ReportingConfiguration
                {
                    Level = FailureLevel.Error
                },
                Name = new Message
                {
                    Text = errorNumber.ToString()
                },
                FullDescription = new Message
                {
                    Text = fullDescription
                },
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    [DefaultRuleMessageId] = new MultiformatMessageString { Text = messageStringWithPath }
                }
            };
        }

        private static readonly Dictionary<ErrorNumber, ReportingDescriptor> s_ruleDictionary = new Dictionary<ErrorNumber, ReportingDescriptor>
        {
            [ErrorNumber.SyntaxError] = MakeRule(
                ErrorNumber.SyntaxError,
                RuleResources.RuleDescriptionSyntaxError,
                RuleResources.ErrorSyntaxError),

            [ErrorNumber.NotAString] = MakeRule(
                ErrorNumber.NotAString,
                RuleResources.RuleDescriptionNotAString,
                RuleResources.ErrorNotAString),

            [ErrorNumber.InvalidAdditionalPropertiesType] = MakeRule(
                ErrorNumber.InvalidAdditionalPropertiesType,
                RuleResources.RuleDescriptionInvalidAdditionalPropertiesType,
                RuleResources.ErrorInvalidAdditionalPropertiesType),

            [ErrorNumber.InvalidItemsType] = MakeRule(
                ErrorNumber.InvalidItemsType,
                RuleResources.RuleDescriptionInvalidItemsType,
                RuleResources.ErrorInvalidItemsType),

            [ErrorNumber.InvalidTypeType] = MakeRule(
                ErrorNumber.InvalidTypeType,
                RuleResources.RuleDescriptionInvalidTypeType,
                RuleResources.ErrorInvalidTypeType),

            [ErrorNumber.InvalidTypeString] = MakeRule(
                ErrorNumber.InvalidTypeString,
                RuleResources.RuleDescriptionInvalidTypeString,
                RuleResources.ErrorInvalidTypeString),

            [ErrorNumber.InvalidAdditionalItemsType] = MakeRule(
                ErrorNumber.InvalidAdditionalItemsType,
                RuleResources.RuleDescriptionInvalidAdditionalItemsType,
                RuleResources.ErrorInvalidAdditionalItemsType),

            [ErrorNumber.InvalidDependencyType] = MakeRule(
                ErrorNumber.InvalidDependencyType,
                RuleResources.RuleDescriptionInvalidDependencyType,
                RuleResources.ErrorInvalidDependencyType),

            [ErrorNumber.InvalidPropertyDependencyType] = MakeRule(
                ErrorNumber.InvalidPropertyDependencyType,
                RuleResources.RuleDescriptionInvalidPropertyDependencyType,
                RuleResources.ErrorInvalidPropertyDependencyType),

            [ErrorNumber.WrongType] = MakeRule(
                ErrorNumber.WrongType,
                RuleResources.RuleDescriptionWrongType,
                RuleResources.ErrorWrongType),

            [ErrorNumber.RequiredPropertyMissing] = MakeRule(
                ErrorNumber.RequiredPropertyMissing,
                RuleResources.RuleDescriptionRequiredPropertyMissing,
                RuleResources.ErrorRequiredPropertyMissing),

            [ErrorNumber.TooFewArrayItems] = MakeRule(
                ErrorNumber.TooFewArrayItems,
                RuleResources.RuleDescriptionTooFewArrayItems,
                RuleResources.ErrorTooFewArrayItems),

            [ErrorNumber.TooManyArrayItems] = MakeRule(
                ErrorNumber.TooManyArrayItems,
                RuleResources.RuleDescriptionTooManyArrayItems,
                RuleResources.ErrorTooManyArrayItems),

            [ErrorNumber.AdditionalPropertiesProhibited] = MakeRule(
                ErrorNumber.AdditionalPropertiesProhibited,
                RuleResources.RuleDescriptionAdditionalPropertiesProhibited,
                RuleResources.ErrorAdditionalPropertiesProhibited),

            [ErrorNumber.ValueTooLarge] = MakeRule(
                ErrorNumber.ValueTooLarge,
                RuleResources.RuleDescriptionValueTooLarge,
                RuleResources.ErrorValueTooLarge),

            [ErrorNumber.ValueTooLargeExclusive] = MakeRule(
                ErrorNumber.ValueTooLargeExclusive,
                RuleResources.RuleDescriptionValueTooLargeExclusive,
                RuleResources.ErrorValueTooLargeExclusive),

            [ErrorNumber.ValueTooSmall] = MakeRule(
                ErrorNumber.ValueTooSmall,
                RuleResources.RuleDescriptionValueTooSmall,
                RuleResources.ErrorValueTooSmall),

            [ErrorNumber.ValueTooSmallExclusive] = MakeRule(
                ErrorNumber.ValueTooSmallExclusive,
                RuleResources.RuleDescriptionValueTooSmallExclusive,
                RuleResources.ErrorValueTooSmallExclusive),

            [ErrorNumber.TooManyProperties] = MakeRule(
                ErrorNumber.TooManyProperties,
                RuleResources.RuleDescriptionTooManyProperties,
                RuleResources.ErrorTooManyProperties),

            [ErrorNumber.TooFewProperties] = MakeRule(
                ErrorNumber.TooFewProperties,
                RuleResources.RuleDescriptionTooFewProperties,
                RuleResources.ErrorTooFewProperties),

            [ErrorNumber.NotAMultiple] = MakeRule(
                ErrorNumber.NotAMultiple,
                RuleResources.RuleDescriptionNotAMultiple,
                RuleResources.ErrorNotAMultiple),

            [ErrorNumber.StringTooLong] = MakeRule(
                ErrorNumber.StringTooLong,
                RuleResources.RuleDescriptionStringTooLong,
                RuleResources.ErrorStringTooLong),

            [ErrorNumber.StringTooShort] = MakeRule(
                ErrorNumber.StringTooShort,
                RuleResources.RuleDescriptionStringTooShort,
                RuleResources.ErrorStringTooShort),

            [ErrorNumber.StringDoesNotMatchPattern] = MakeRule(
                ErrorNumber.StringDoesNotMatchPattern,
                RuleResources.RuleDescriptionStringDoesNotMatchPattern,
                RuleResources.ErrorStringDoesNotMatchPattern),

            [ErrorNumber.NotAllOf] = MakeRule(
                ErrorNumber.NotAllOf,
                RuleResources.RuleDescriptionNotAllOf,
                RuleResources.ErrorNotAllOf),

            [ErrorNumber.NotAnyOf] = MakeRule(
                ErrorNumber.NotAnyOf,
                RuleResources.RuleDescriptionNotAnyOf,
                RuleResources.ErrorNotAnyOf),

            [ErrorNumber.NotOneOf] = MakeRule(
                ErrorNumber.NotOneOf,
                RuleResources.RuleDescriptionNotOneOf,
                RuleResources.ErrorNotOneOf),

            [ErrorNumber.InvalidEnumValue] = MakeRule(
                ErrorNumber.InvalidEnumValue,
                RuleResources.RuleDescriptionInvalidEnumValue,
                RuleResources.ErrorInvalidEnumValue),

            [ErrorNumber.NotUnique] = MakeRule(
                ErrorNumber.NotUnique,
                RuleResources.RuleDescriptionNotUnique,
                RuleResources.ErrorNotUnique),

            [ErrorNumber.TooFewItemSchemas] = MakeRule(
                ErrorNumber.TooFewItemSchemas,
                RuleResources.RuleDescriptionTooFewItemSchemas,
                RuleResources.ErrorTooFewItemSchemas),

            [ErrorNumber.ValidatesAgainstNotSchema] = MakeRule(
                ErrorNumber.ValidatesAgainstNotSchema,
                RuleResources.RuleDescriptionValidatesAgainstNotSchema,
                RuleResources.ErrorValidatesAgainstNotSchema),

            [ErrorNumber.DependentPropertyMissing] = MakeRule(
                ErrorNumber.DependentPropertyMissing,
                RuleResources.RuleDescriptionDependentPropertyMissing,
                RuleResources.ErrorDependentPropertyMissing)
        };

        public static ReportingDescriptor GetRuleFromRuleId(string ruleId)
        {
            var errorNumber = (ErrorNumber)int.Parse(ruleId.Substring(ErrorCodePrefix.Length));
            return GetRuleFromErrorNumber(errorNumber);
        }

        public static ReportingDescriptor GetRuleFromErrorNumber(ErrorNumber errorNumber)
        {
            return s_ruleDictionary[errorNumber];
        }
    }
}
