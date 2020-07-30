// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.Validation
{
    /// <summary>
    /// Validates a JSON instance against a schema.
    /// </summary>
    public class Validator
    {
        private const string Null = "null";

        private readonly Stack<JsonSchema> _schemas;
        private List<Result> _results;
        private Dictionary<string, JsonSchema> _definitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        /// <param name="schema">
        /// The JSON schema to use for validation.
        /// </param>
        public Validator(JsonSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            _definitions = schema.Definitions;

            schema = Resolve(schema);

            _results = new List<Result>();

            _schemas = new Stack<JsonSchema>();
            _schemas.Push(schema);
        }

        public Result[] Validate(string instanceText, string instanceFilePath)
        {
            _results = new List<Result>();

            using (var reader = new StringReader(instanceText))
            {
                JToken token;
                try
                {
                    token = JToken.ReadFrom(new JsonTextReader(reader));
                }
                catch (JsonReaderException ex)
                {
                    throw new JsonSyntaxException(instanceFilePath, ex);
                }

                JsonSchema schema = _schemas.Peek();

                ValidateToken(token, schema);
            }

            foreach (Result result in _results)
            {
                result.SetResultFile(instanceFilePath);
            }

            return _results.ToArray();
        }

        private JsonSchema Resolve(JsonSchema schema)
        {
            if (schema.Reference != null)
            {
                schema = _definitions[schema.Reference.GetDefinitionName()];
            }

            return schema;
        }

        private void ValidateToken(JToken jToken, JsonSchema schema)
        {
            if (!TokenTypeIsCompatibleWithSchema(jToken.Type, schema.Type))
            {
                AddResult(jToken, ErrorNumber.WrongType, FormatSchemaTypes(schema.Type), jToken.Type);
                return;
            }

            switch (jToken.Type)
            {
                case JTokenType.String:
                    ValidateString((JValue)jToken, schema);
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                    ValidateNumber((JValue)jToken, schema);
                    break;

                case JTokenType.Object:
                    ValidateObject((JObject)jToken, schema);
                    break;

                case JTokenType.Array:
                    ValidateArray((JArray)jToken, schema);
                    break;

                default:
                    break;
            }

            if (schema.Enum != null)
            {
                ValidateEnum(jToken, schema.Enum);
            }

            if (schema.AllOf != null)
            {
                ValidateAllOf(jToken, schema.AllOf);
            }

            if (schema.AnyOf != null)
            {
                ValidateAnyOf(jToken, schema.AnyOf);
            }

            if (schema.OneOf != null)
            {
                ValidateOneOf(jToken, schema.OneOf);
            }

            if (schema.Not != null)
            {
                ValidateNot(jToken, schema.Not);
            }
        }

        private static bool TokenTypeIsCompatibleWithSchema(JTokenType instanceType, IList<SchemaType> schemaTypes)
        {
            return schemaTypes == null
                || !schemaTypes.Any()
                || schemaTypes.Contains(instanceType.ToSchemaType())
                || (instanceType == JTokenType.Integer && schemaTypes.Contains(SchemaType.Number));
        }

        private void ValidateString(JValue jValue, JsonSchema schema)
        {
            if (schema.MaxLength.HasValue)
            {
                string value = jValue.Value<string>();

                if (UnicodeLength(value) > schema.MaxLength)
                {
                    AddResult(jValue, ErrorNumber.StringTooLong, value, value.Length, schema.MaxLength);
                }
            }

            if (schema.MinLength.HasValue)
            {
                string value = jValue.Value<string>();

                if (UnicodeLength(value) < schema.MinLength)
                {
                    AddResult(jValue, ErrorNumber.StringTooShort, value, value.Length, schema.MinLength);
                }
            }

            if (schema.Pattern != null)
            {
                string value = jValue.Value<string>();

                if (!Regex.IsMatch(value, schema.Pattern))
                {
                    AddResult(jValue, ErrorNumber.StringDoesNotMatchPattern, value, schema.Pattern);
                }
            }
        }

        // Compute the length of a string, counting surrogate pairs as one
        // character (String.Length counts them as two characters).
        private int UnicodeLength(string value)
        {
            return value.Where(c => !char.IsLowSurrogate(c)).Count();
        }

        private void ValidateNumber(JValue jValue, JsonSchema schema)
        {
            if (schema.Maximum.HasValue)
            {
                double maximum = schema.Maximum.Value;
                double value = jValue.Type == JTokenType.Float
                    ? (double)jValue.Value
                    : (long)jValue.Value;

                if (schema.ExclusiveMaximum == true && value >= maximum)
                {
                    AddResult(jValue, ErrorNumber.ValueTooLargeExclusive, value, maximum);
                }
                else if (value > maximum)
                {
                    AddResult(jValue, ErrorNumber.ValueTooLarge, value, maximum);
                }
            }

            if (schema.Minimum.HasValue)
            {
                double minimum = schema.Minimum.Value;
                double value = jValue.Type == JTokenType.Float
                    ? (double)jValue.Value
                    : (long)jValue.Value;

                if (schema.ExclusiveMinimum == true && value <= minimum)
                {
                    AddResult(jValue, ErrorNumber.ValueTooSmallExclusive, value, minimum);
                }
                else if (value < minimum)
                {
                    AddResult(jValue, ErrorNumber.ValueTooSmall, value, minimum);
                }
            }

            if (schema.MultipleOf.HasValue)
            {
                double factor = schema.MultipleOf.Value;
                double value = jValue.Type == JTokenType.Float
                    ? (double)jValue.Value
                    : (long)jValue.Value;

                double quotient = value / factor;
                if (quotient != (int)quotient)
                {
                    AddResult(jValue, ErrorNumber.NotAMultiple, value, factor);
                }
            }
        }

        private void ValidateArray(JArray jArray, JsonSchema schema)
        {
            int numItems = jArray.Count;
            if (numItems < schema.MinItems)
            {
                AddResult(jArray, ErrorNumber.TooFewArrayItems, schema.MinItems, numItems);
            }

            if (numItems > schema.MaxItems)
            {
                AddResult(jArray, ErrorNumber.TooManyArrayItems, schema.MaxItems, numItems);
            }

            if (schema.UniqueItems == true)
            {
                ValidateUnique(jArray);
            }

            // 5.3.1.3 The only case where the array itself can fail to validate (as
            // opposed to the array elements) is if "additionalItems" is false and
            // "items" is an array.
            if (schema.Items != null &&
                !schema.Items.SingleSchema &&
                schema.AdditionalItems != null &&
                !schema.AdditionalItems.Allowed &&
                jArray.Count > schema.Items.Schemas.Count)
            {
                AddResult(jArray, ErrorNumber.TooFewItemSchemas, jArray.Count, schema.Items.Schemas.Count);
            }

            ValidateArrayItems(jArray, schema);
        }

        private void ValidateArrayItems(JArray jArray, JsonSchema schema)
        {
            if (schema.Items == null)
            {
                return;
            }

            JToken[] arrayElements = jArray.ToArray();

            if (schema.Items.SingleSchema == true)
            {
                // 8.2.3.1 If "items" is a schema, then each element must be valid against
                // this schema, regardless of its index, and regardless of the value of
                // "additionalItems".
                foreach (JToken jToken in jArray)
                {
                    ValidateToken(jToken, Resolve(schema.Items.Schema));
                }
            }
            else
            {
                // 8.2.3.2 If "items" is an array, then:
                JsonSchema[] arrayElementSchemas = schema.Items.Schemas
                    .Select(s => Resolve(s))
                    .ToArray();

                int numElementsWithSchemas = Math.Min(arrayElements.Length, arrayElementSchemas.Length);
                for (int i = 0; i < numElementsWithSchemas; ++i)
                {
                    ValidateToken(arrayElements[i], arrayElementSchemas[i]);
                }

                if (schema.AdditionalItems?.Schema != null)
                {
                    for (int i = arrayElementSchemas.Length; i < arrayElements.Length; ++i)
                    {
                        ValidateToken(arrayElements[i], schema.AdditionalItems.Schema);
                    }
                }
            }
        }

        private void ValidateObject(JObject jObject, JsonSchema schema)
        {
            List<string> instancePropertySet = GetPropertyNames(jObject);

            if (schema.MaxProperties.HasValue &&
                instancePropertySet.Count > schema.MaxProperties.Value)
            {
                AddResult(jObject, ErrorNumber.TooManyProperties, schema.MaxProperties.Value, instancePropertySet.Count);
            }

            if (schema.MinProperties.HasValue &&
                instancePropertySet.Count < schema.MinProperties.Value)
            {
                AddResult(jObject, ErrorNumber.TooFewProperties, schema.MinProperties.Value, instancePropertySet.Count);
            }

            // Ensure required properties are present.
            if (schema.Required != null)
            {
                IEnumerable<string> missingProperties = schema.Required.Except(instancePropertySet);
                foreach (string propertyName in missingProperties)
                {
                    AddResult(jObject, ErrorNumber.RequiredPropertyMissing, propertyName);
                }
            }

            List<string> propertiesPropertySet = schema.Properties != null
                ? schema.Properties.Keys.ToList()
                : new List<string>();

            ValidateObjectPropertyNames(jObject, instancePropertySet, propertiesPropertySet, schema);

            ValidateObjectPropertyValues(jObject, instancePropertySet, propertiesPropertySet, schema);

            ValidateDependencies(jObject, schema, instancePropertySet);
        }

        private List<string> GetPropertyNames(JObject jObject)
        {
            return jObject.Properties().Select(p => p.Name).ToList();
        }

        // Validate the set of property names on an object.
        // 5.4.4. additionalProperties, properties, and patternProperties
        private void ValidateObjectPropertyNames(
            JObject jObject,
            List<string> instancePropertySet,
            List<string> propertiesPropertySet,
            JsonSchema schema)
        {
            // 5.4.4.2 If "additionalProperties" is boolean true or a schema, validation
            // succeeds.
            //
            // In the object model, AdditionalProperties.Allowed will be true if and only
            // if either
            // (a) "additionalProperties" was boolean true, or
            // (b) "additionalProperties" was a schema.
            // ... which is exactly what 5.4.4.2 requires.
            //
            // 5.4.4.3 If "additionalProperties" is absent, it may be considered present
            // with an empty schema as a value [in which case, validation succeeds].
            if (schema.AdditionalProperties == null ||
                schema.AdditionalProperties.Allowed == true)
            {
                return;
            }

            // From the property set of the instance to validate, remove all elements
            // of the property set from "properties".
            IEnumerable<string> unexpectedPropertySet = instancePropertySet.Except(propertiesPropertySet);

            // For each regex in "patternProperties", remove all elements which this
            // regex matches.
            if (schema.PatternProperties != null)
            {
                foreach (string patternRegEx in schema.PatternProperties.Keys)
                {
                    unexpectedPropertySet = unexpectedPropertySet.Where(p => !Regex.IsMatch(p, patternRegEx));
                }
            }

            // Validation of the instance succeeds if, after these steps, no elements remain.
            foreach (string unexpectedPropertyName in unexpectedPropertySet)
            {
                AddResult(
                    jObject.Property(unexpectedPropertyName),
                    ErrorNumber.AdditionalPropertiesProhibited,
                    unexpectedPropertyName);
            }
        }

        // Validate the set of members of an object.
        // 8.3 Object members.
        private void ValidateObjectPropertyValues(
            JObject jObject,
            List<string> instancePropertySet,
            List<string> propertiesPropertySet,
            JsonSchema schema)
        {
            foreach (string instancePropertyName in instancePropertySet)
            {
                ValidateObjectPropertyValue(jObject, instancePropertyName, propertiesPropertySet, schema);
            }
        }

        private void ValidateObjectPropertyValue(
            JObject jObject,
            string instancePropertyName,
            List<string> propertiesPropertySet,
            JsonSchema schema)
        {
            // First ascertain the set of schemas against which this property must validate
            // successfully.
            var applicableSchemas = new List<JsonSchema>();

            // 8.3.3.2 If the property name appears in "properties", add the corresponding schema.
            if (propertiesPropertySet.Contains(instancePropertyName))
            {
                applicableSchemas.Add(Resolve(schema.Properties[instancePropertyName]));
            }

            // 8.3.3.3 For each regex in "patternProperties", if it matches the property
            // name, add the corresponding schema.
            if (schema.PatternProperties != null)
            {
                foreach (string regex in schema.PatternProperties.Keys)
                {
                    if (Regex.IsMatch(instancePropertyName, regex))
                    {
                        applicableSchemas.Add(Resolve(schema.PatternProperties[regex]));
                    }
                }
            }

            // 8.3.3.4 Add the schema defined by "additionalProperties" if and only if
            // there are not yet any applicable schemas.
            if (!applicableSchemas.Any())
            {
                if (schema.AdditionalProperties?.Schema != null)
                {
                    applicableSchemas.Add(Resolve(schema.AdditionalProperties.Schema));
                }
            }

            // Now validate the property against all applicable schemas.
            foreach (JsonSchema applicableSchema in applicableSchemas)
            {
                ValidateToken(jObject.Property(instancePropertyName).Value, applicableSchema);
            }
        }

        private void ValidateDependencies(
            JObject jObject,
            JsonSchema schema,
            IList<string> instancePropertySet)
        {
            if (schema.Dependencies == null)
            {
                return;
            }

            foreach (string key in schema.Dependencies.Keys)
            {
                Dependency dependency = schema.Dependencies[key];
                ValidateDependency(jObject, key, dependency, instancePropertySet);
            }
        }

        private void ValidateDependency(
            JObject jObject,
            string propertyName,
            Dependency dependency,
            IList<string> instancePropertySet)
        {
            if (dependency.SchemaDependency != null)
            {
                ValidateSchemaDependency(jObject, propertyName, dependency.SchemaDependency, instancePropertySet);
            }

            if (dependency.PropertyDependencies != null)
            {
                ValidatePropertyDependencies(jObject, propertyName, dependency.PropertyDependencies, instancePropertySet);
            }
        }

        private void ValidateSchemaDependency(
            JObject jObject,
            string propertyName,
            JsonSchema schemaDependency,
            IList<string> instancePropertySet)
        {
            if (instancePropertySet.Contains(propertyName))
            {
                // TODO: Be more specific: Report the error that a dependency
                // was violated.
                ValidateObject(jObject, Resolve(schemaDependency));
            } 
        }

        private void ValidatePropertyDependencies(
            JObject jObject,
            string propertyName,
            List<string> propertyDependencies,
            IList<string> instancePropertySet)
        {
            if (instancePropertySet.Contains(propertyName))
            {
                List<string> missingDependencies = propertyDependencies
                    .Except(instancePropertySet)
                    .ToList();

                if (missingDependencies.Any())
                {
                    AddResult(
                        jObject,
                        ErrorNumber.DependentPropertyMissing,
                        propertyName,
                        FormatList(propertyDependencies.Cast<object>().ToList()),
                        FormatList(missingDependencies.Cast<object>().ToList()));
                }
            }
        }

        private void ValidateEnum(JToken jToken, IList<object> @enum)
        {
            if (!TokenMatchesEnum(jToken, @enum))
            {
                AddResult(
                    jToken,
                    ErrorNumber.InvalidEnumValue,
                    FormatObject(jToken),
                    FormatList(@enum));
            }
        }

        private void ValidateAllOf(
            JToken jToken,
            IList<JsonSchema> allOfSchemas)
        {
            var allOfResults = new List<Result>();

            foreach (JsonSchema allOfSchema in allOfSchemas)
            {
                JsonSchema schema = Resolve(allOfSchema);
                var allOfValidator = new Validator(schema);
                allOfValidator.ValidateToken(jToken, schema);
                allOfResults.AddRange(allOfValidator._results);
            }

            if (allOfResults.Any())
            {
                AddResult(jToken, ErrorNumber.NotAllOf, allOfSchemas.Count);
            }
        }

        private void ValidateAnyOf(
            JToken jToken,
            IList<JsonSchema> anyOfSchemas)
        {
            bool valid = false;

            // Since this token is valid if it's valid against *any* of the schemas,
            // we can't just call ValidateToken against each schema. If we did that,
            // we'd accumulate errors from all the failed schemas, only to find out
            // that they weren't really errors at all. So we instantiate a new
            // validator for each schema, and only report the errors from the current
            // validator if they *all* fail.
            foreach (JsonSchema anyOfSchema in anyOfSchemas)
            {
                JsonSchema schema = Resolve(anyOfSchema);
                var anyOfValidator = new Validator(schema);
                anyOfValidator.ValidateToken(jToken, schema);
                if (!anyOfValidator._results.Any())
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
            {
                AddResult(jToken, ErrorNumber.NotAnyOf, anyOfSchemas.Count);
            }
        }

        private void ValidateOneOf(
            JToken jToken,
            IList<JsonSchema> oneOfSchemas)
        {
            int numValid = 0;

            // Since this token is valid if it's valid against *exactly one* of the schemas,
            // we can't just call ValidateToken against each schema. If we did that,
            // we'd accumulate errors from all the failed schemas, only to find out
            // that they weren't really errors at all. So we instantiate a new
            // validator for each schema, and only report an error from the current
            // validator if *all but one* fail.
            foreach (JsonSchema oneOfSchema in oneOfSchemas)
            {
                JsonSchema schema = Resolve(oneOfSchema);
                var oneOfValidator = new Validator(schema);
                oneOfValidator.ValidateToken(jToken, schema);
                if (!oneOfValidator._results.Any())
                {
                    ++numValid;
                }
            }

            if (numValid != 1)
            {
                AddResult(jToken, ErrorNumber.NotOneOf, numValid, oneOfSchemas.Count);
            }
        }

        private void ValidateNot(JToken jToken, JsonSchema notSchema)
        {
            // Since this token is valid if it's *not* valid against the schema, we can't
            // just call ValidateToken against this schema. If we did that, we'd
            // accumulate and report the errors from this schema, even though we want
            // there to be at least one error. So we instantiate a new validator for this
            // schema, and only report an error from the current validator if
            // validation against this schema *succeeds*.
            JsonSchema schema = Resolve(notSchema);
            var notValidator = new Validator(schema);
            notValidator.ValidateToken(jToken, schema);
            if (!notValidator._results.Any())
            {
                AddResult(jToken, ErrorNumber.ValidatesAgainstNotSchema);
            }
        }

        private void ValidateAdditionalProperties(
            JObject jObject,
            List<string> additionalPropertyNames,
            AdditionalProperties additionalProperties)
        {
            if (!additionalProperties.Allowed)
            {
                foreach (string propertyName in additionalPropertyNames)
                {
                    JProperty property = jObject.Property(propertyName);
                    AddResult(property, ErrorNumber.AdditionalPropertiesProhibited, propertyName);
                }
            }
            else
            {
                // Additional properties are allowed. If there is a schema to which they
                // must conform, ensure that they do.
                if (additionalProperties.Schema != null)
                {
                    JsonSchema resolvedSchema = Resolve(additionalProperties.Schema);
                    foreach (string propertyName in additionalPropertyNames)
                    {
                        JProperty property = jObject.Property(propertyName);
                        ValidateToken(property.Value, resolvedSchema);
                    }
                }
            }
        }

        private void ValidateUnique(JArray jArray)
        {
            if (jArray.Distinct(JTokenEqualityComparer.Instance).Count() != jArray.Count)
            {
                AddResult(jArray, ErrorNumber.NotUnique);
            }
        }

        private void AddResult(JToken jToken, ErrorNumber errorNumber, params object[] args)
        {
            _results.Add(ResultFactory.CreateResult(jToken, errorNumber, args));
        }

        private bool TokenMatchesEnum(JToken jToken, IList<object> @enum)
        {
            return @enum.Any(e => JTokenEqualityComparer.DeepEquals(jToken, e));
        }

        private static string FormatSchemaTypes(IList<SchemaType> schemaTypes)
        {
            return string.Join(", ", schemaTypes.Select(t => t.ToString()));
        }

        private static string FormatList(IList<object> objects)
        {
            return string.Join(", ", objects.Select(e => FormatObject(e)));
        }

        private static string FormatObject(object obj)
        {
            if (obj is JToken)
            {
                return FormatToken(obj as JToken);
            }

            if (obj == null)
            {
                return Null;
            }

            string formattedObject = obj.ToString();

            if (obj is string)
            {
                formattedObject = FormatQuotedString(formattedObject);
            }
            else if (obj is bool)
            {
                formattedObject = FormatBoolean(formattedObject);
            }

            return formattedObject;
        }

        private static string FormatToken(JToken jToken)
        {
            string formattedToken = jToken.ToString();

            switch (jToken.Type)
            {
                case JTokenType.String:
                    formattedToken = FormatQuotedString(formattedToken);
                    break;

                case JTokenType.Boolean:
                    formattedToken = FormatBoolean(formattedToken);
                    break;

                case JTokenType.Array:
                    formattedToken = FormatArray(formattedToken);
                    break;

                case JTokenType.Null:
                    formattedToken = Null;
                    break;

                default:
                    break;
            }

            return formattedToken;
        }

        private static string FormatQuotedString(string s)
        {
            return '"' + s + '"';
        }

        private static string FormatBoolean(string s)
        {
            return s.ToLowerInvariant();
        }

        private static string FormatArray(string s)
        {
            s = Regex.Replace(s, @"\[\s+", @"[");
            s = Regex.Replace(s, @",\s+", ", ");
            s = Regex.Replace(s, @"\s+\]", "]");

            return s;
        }
    }
}
