// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Json.Schema
{
    public class JsonSchema : IEquatable<JsonSchema>
    {
        public static readonly Uri V4Draft = new Uri("http://json-schema.org/draft-04/schema#");

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchema"/> class.
        /// </summary>
        public JsonSchema()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchema"/> class from the
        /// specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance used to initialize the new instance.
        /// </param>
        public JsonSchema(JsonSchema other)
        {
            if (other.Id != null)
            {
                Id = new UriOrFragment(other.Id);
            }

            if (other.SchemaVersion != null)
            {
                SchemaVersion = new Uri(other.SchemaVersion.OriginalString);
            }

            Title = other.Title;
            Description = other.Description;

            if (other.Type != null)
            {
                Type = new List<SchemaType>(other.Type);
            }

            if (other.Enum != null)
            {
                // A shallow copy is fine for now since Enum will be checked to ensure that it
                // contain only primitive types.
                // But it won't do if we want to pass the JSON Schema validation suite.
                Enum = new List<object>(other.Enum);
            }

            if (other.Items != null)
            {
                Items = new Items(other.Items);
            }

            if (other.Properties != null)
            {
                Properties = new Dictionary<string, JsonSchema>();
                foreach (string key in other.Properties.Keys)
                {
                    Properties.Add(key, new JsonSchema(other.Properties[key]));
                }
            }

            if (other.Required != null)
            {
                Required = new List<string>(other.Required);
            }

            if (other.Definitions != null)
            {
                Definitions = new Dictionary<string, JsonSchema>();
                foreach (string key in other.Definitions.Keys)
                {
                    Definitions.Add(key, new JsonSchema(other.Definitions[key]));
                }
            }

            if (other.AdditionalProperties != null)
            {
                AdditionalProperties = new AdditionalProperties(other.AdditionalProperties);
            }

            if (other.PatternProperties != null)
            {
                PatternProperties = new Dictionary<string, JsonSchema>(other.PatternProperties);
            }

            Pattern = other.Pattern;
            MaxLength = other.MaxLength;
            MinLength = other.MinLength;
            Format = other.Format;
            MultipleOf = other.MultipleOf;
            Maximum = other.Maximum;
            ExclusiveMaximum = other.ExclusiveMaximum;
            MinItems = other.MinItems;
            MaxItems = other.MaxItems;
            UniqueItems = other.UniqueItems;

            if (other.Reference != null)
            {
                Reference = new UriOrFragment(other.Reference);
            }

            if (other.AllOf != null)
            {
                AllOf = new List<JsonSchema>(other.AllOf);
            }

            if (other.AnyOf != null)
            {
                AnyOf = new List<JsonSchema>(other.AnyOf);
            }

            if (other.OneOf != null)
            {
                OneOf = new List<JsonSchema>(other.OneOf);
            }

            if (other.Not != null)
            {
                Not = new JsonSchema(other.Not);
            }
        }

        /// <summary>
        /// Gets or sets a URI that alters the resolution scope for the current schema and
        /// all of its descendants (until another Id is encountered).
        /// </summary>
        /// <remarks>
        /// See http://json-schema.org/latest/json-schema-core.html#anchor25 ("URI resolution
        /// scopes and dereferencing").
        /// </remarks>
        public UriOrFragment Id { get; set; }

        [JsonProperty("$schema")]
        public Uri SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets a short string describing this schema. See Sec. 6.1
        /// </summary>
        /// <remarks>
        /// The JSON schema spec requires <code>title<code> to be a string. Don't let
        /// Json.NET just serialize anything it finds into a string.
        /// </remarks>
        [JsonConverter(typeof(MustBeStringConverter))]
        public string Title { get; set; }


        /// <summary>
        /// Gets or sets a string describing this schema, including the purpose of
        /// instances described by this schema. See Sec. 6.1.
        /// </summary>
        /// <remarks>
        /// The JSON schema spec requires <code>title<code> to be a string. Don't let
        /// Json.NET just serialize anything it finds into a string.
        /// </remarks>
        [JsonConverter(typeof(MustBeStringConverter))]
        public string Description { get; set; }

        [JsonConverter(typeof(SchemaTypeConverter))]
        public IList<SchemaType> Type { get; set; }

        /// <summary>
        /// Gets or sets an array containing the values that are valid for an object
        /// that conforms to the current schema.
        /// </summary>
        public IList<object> Enum { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema or schemas that applies to the array items, if the current
        /// schema is of array type.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Array"/>.
        /// </remarks>
        public Items Items { get; set; }

        /// <summary>
        /// Gets or sets the maximum valid number of properties.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Object"/>.
        /// </remarks>
        public int? MaxProperties { get; set; }

        /// <summary>
        /// Gets or sets the maximum valid number of properties.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Object"/>.
        /// </remarks>
        public int? MinProperties { get; set; }

        /// <summary>
        /// Gets or sets an array containing the names of the property names that are
        /// required to be present.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Object"/>.
        /// </remarks>
        public IList<string> Required { get; set; }

        /// <summary>
        /// Gets or sets a value describing any additional properties allowed by the
        /// schema.
        /// </summary>
        public AdditionalProperties AdditionalProperties { get; set; }

        /// <summary>
        /// Dictionary mapping valid property names to the sub-schemas to which they must
        /// conform.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Object"/>.
        /// </remarks>
        public Dictionary<string, JsonSchema> Properties { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that maps a set of regular expressions to the schemas
        /// to which properties whose names match those regular expressions must conform.
        /// </summary>
        public IDictionary<string, JsonSchema> PatternProperties { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of a string schema instance.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.String"/>.
        /// </remarks>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of a string schema instance.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.String"/>.
        /// </remarks>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets a regular expression which a string schema instance must match.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.String"/>.
        /// </remarks>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets a value of which a numeric schema instance must be a multiple.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Integer"/>
        /// or <see cref="SchemaType.Number"/>.
        /// </remarks>
        public double? MultipleOf { get; set; }

        /// <summary>
        /// Gets or sets the maximum valid value.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Integer"/>
        /// or <see cref="SchemaType.Number"/>.
        /// </remarks>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value specified by <see cref="Maximum"/>
        /// is an exclusive maximum.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Integer"/>
        /// or <see cref="SchemaType.Number"/>. If not specified in the schema, the default
        /// value is <code>false</code>.
        /// </remarks>
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>
        /// Gets or sets the minimum valid value.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Integer"/>
        /// or <see cref="SchemaType.Number"/>.
        /// </remarks>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value specified by <see cref="Minimum"/>
        /// is an exclusive minimum.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Integer"/>
        /// or <see cref="SchemaType.Number"/>. If not specified in the schema, the default
        /// value is <code>false</code>.
        /// </remarks>
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping schema names to sub-schemas which can be
        /// referenced by properties defined elsewhere in the current schema.
        /// </summary>
        public Dictionary<string, JsonSchema> Definitions { get; set; }

        /// <summary>
        /// Gets or sets the minimum valid number of elements in an array.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Array"/>.
        /// If this property is not specified, it is considered present with a value of 0.
        /// The type of this property is <code>int?</code>, rather than <code>int</code>
        /// with a default value of 0, so that a schema that does not specify this
        /// property can be successfully round-tripped to and from the object model.
        /// </remarks>
        public int? MinItems { get; set; }

        /// <summary>
        /// Gets or sets the maximum valid number of elements in an array.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Array"/>.
        /// If this property is not specified, any number of items is valid.
        /// The type of this property is <code>int?</code>, rather than <code>int</code>
        /// with a default value of <code>Int32.MaxValue</code>, so that a schema that
        /// does not specify this property can be successfully round-tripped to and from
        /// the object model.
        public int? MaxItems { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether array elements must be unique.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.Array"/>.
        /// If this property is not specified, the default value is <code>false</code>.
        /// The type of this property is <code>bool?</code>, rather than <code>bool</code>
        /// with a default value of <code>false</code>, so that a schema that does not specify
        /// this property can be successfully round-tripped to and from the object model.
        /// </remarks>
        public bool? UniqueItems { get; set; }

        /// <summary>
        /// Gets or sets the URI of a schema that is incorporated by reference into
        /// the current schema.
        /// </summary>
        // See the RefProperty class for an explanation of our special treatment of
        // this property.
        [JsonProperty("$$ref")]
        public UriOrFragment Reference { get; set; }

        /// <summary>
        /// Gets or sets a string specifying the required format of a string-valued property.
        /// </summary>
        /// <remarks>
        /// This property applies only to schemas whose <see cref="Type"/> is <see cref="SchemaType.String"/>.
        /// </remarks>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets a set of schemas against all of which the instance must validate successfully.
        /// </summary>
        public IList<JsonSchema> AllOf { get; set; }

        /// <summary>
        /// Gets or sets a set of schemas against any of which the instance must validate successfully.
        /// </summary>
        public IList<JsonSchema> AnyOf { get; set; }

        /// <summary>
        /// Gets or sets a set of schemas against exactly one of which the instance must validate successfully.
        /// </summary>
        public IList<JsonSchema> OneOf { get; set; }

        /// <summary>
        /// Gets or sets a schemas against which the instance must not validate successfully.
        /// </summary>
        public JsonSchema Not { get; set; }

        /// <summary>
        /// Pull properties from any referenced schemas up into this schema.
        /// </summary>
        public static JsonSchema Collapse(JsonSchema schema)
        {
            return Collapse(schema, schema);
        }

        private static JsonSchema Collapse(JsonSchema schema, JsonSchema rootSchema)
        {
            JsonSchema collapsedSchema = new JsonSchema(schema);

            if (schema.Items != null)
            {
                collapsedSchema.Items = new Items(
                    new List<JsonSchema>(
                        schema.Items.Schemas.Select(
                            itemSchema => Collapse(itemSchema, rootSchema))));
                collapsedSchema.Items.SingleSchema = schema.Items.SingleSchema;
            }

            if (schema.Properties != null)
            {
                collapsedSchema.Properties = new Dictionary<string, JsonSchema>();
                foreach (string key in schema.Properties.Keys)
                {
                    collapsedSchema.Properties.Add(
                        key, Collapse(schema.Properties[key], rootSchema));
                }
            }

            if (schema.Definitions != null)
            {
                collapsedSchema.Definitions = new Dictionary<string, JsonSchema>();
                foreach (string key in schema.Definitions.Keys)
                {
                    collapsedSchema.Definitions.Add(
                        key, Collapse(schema.Definitions[key], rootSchema));
                }
            }

            if (schema.AdditionalProperties?.Schema != null)
            {
                collapsedSchema.AdditionalProperties = new AdditionalProperties(
                    Collapse(schema.AdditionalProperties?.Schema, rootSchema));
            }

            if (schema.PatternProperties != null)
            {
                collapsedSchema.PatternProperties = new Dictionary<string, JsonSchema>();
                foreach (string key in schema.PatternProperties.Keys)
                {
                    collapsedSchema.PatternProperties.Add(
                        key, Collapse(schema.PatternProperties[key], rootSchema));
                }
            }

            if (schema.AllOf != null)
            {
                collapsedSchema.AllOf = schema.AllOf.Select(s => Collapse(s, rootSchema)).ToList();
            }

            if (schema.AnyOf != null)
            {
                collapsedSchema.AnyOf = schema.AnyOf.Select(s => Collapse(s, rootSchema)).ToList();
            }

            if (schema.OneOf != null)
            {
                collapsedSchema.OneOf = schema.OneOf.Select(s => Collapse(s, rootSchema)).ToList();
            }

            if (schema.Not != null)
            {
                collapsedSchema.Not = Collapse(schema.Not, rootSchema);
            }

            if (schema.Reference != null)
            {
                if (!schema.Reference.IsFragment)
                {
                    throw Error.CreateException(
                        Resources.ErrorOnlyDefinitionFragmentsSupported,
                        schema.Reference);
                }

                string definitionName = schema.Reference.GetDefinitionName();

                JsonSchema referencedSchema;
                if (rootSchema.Definitions == null || !rootSchema.Definitions.TryGetValue(definitionName, out referencedSchema))
                {
                    throw Error.CreateException(
                        Resources.ErrorDefinitionDoesNotExist,
                        definitionName);
                }

                if (referencedSchema.Type != null)
                {
                    collapsedSchema.Type = new List<SchemaType>(referencedSchema.Type);
                }

                if (referencedSchema.Enum != null)
                {
                    collapsedSchema.Enum = new List<object>(referencedSchema.Enum);
                }

                if (referencedSchema.Items != null)
                {
                    collapsedSchema.Items = new Items(
                        new List<JsonSchema>(
                            referencedSchema.Items.Schemas.Select(
                                itemSchema => Collapse(itemSchema, rootSchema))));
                    collapsedSchema.Items.SingleSchema = referencedSchema.Items.SingleSchema;
                }

                collapsedSchema.Pattern = referencedSchema.Pattern;
                collapsedSchema.MaxLength = referencedSchema.MaxLength;
                collapsedSchema.MinLength = referencedSchema.MinLength;
                collapsedSchema.MultipleOf = referencedSchema.MultipleOf;
                collapsedSchema.Maximum = referencedSchema.Maximum;
                collapsedSchema.ExclusiveMaximum = referencedSchema.ExclusiveMaximum;
                collapsedSchema.MinItems = referencedSchema.MinItems;
                collapsedSchema.MaxItems = referencedSchema.MaxItems;
                collapsedSchema.UniqueItems = referencedSchema.UniqueItems;
                collapsedSchema.Format = referencedSchema.Format;
            }

            return collapsedSchema;
        }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonSchema);
        }

        public override int GetHashCode()
        {
            List<object> hashContributors = new List<object>
            {
                Id,
                SchemaVersion,
                Title,
                Description,
                Enum,
                Items,
                Properties,
                Required,
                Definitions,
                Reference,
                Pattern,
                MaxLength,
                MinLength,
                MultipleOf,
                Maximum,
                ExclusiveMaximum,
                MinItems,
                MaxItems,
                UniqueItems,
                Format,
                AllOf,
                AnyOf,
                OneOf,
                Not
            };

            hashContributors.AddRange(Type.Cast<object>());

            return Hash.Combine(hashContributors);
        }

        #endregion Object overrides

        #region IEquatable<T>

        public bool Equals(JsonSchema other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return Id == other.Id
                && (SchemaVersion == null
                        ? other.SchemaVersion == null
                        : SchemaVersion.EqualsWithFragments(other.SchemaVersion))
                && string.Equals(Title, other.Title, StringComparison.Ordinal)
                && string.Equals(Description, other.Description, StringComparison.Ordinal)
                && Type.HasSameElementsAs(other.Type)
                && Enum.HasSameElementsAs(other.Enum)
                && (Items == null
                        ? other.Items == null
                        : Items.Equals(other.Items))
                && (Properties == null
                        ? other.Properties == null
                        : Properties.HasSameElementsAs(other.Properties))
                && Required.HasSameElementsAs(other.Required)
                && Definitions.HasSameElementsAs(other.Definitions)
                && (AdditionalProperties == null
                        ? other.AdditionalProperties == null
                        : AdditionalProperties.Equals(other.AdditionalProperties))
                && (PatternProperties == null
                        ? other.PatternProperties == null
                        : PatternProperties.HasSameElementsAs(other.PatternProperties))
                && (Reference == null
                        ? other.Reference == null
                        : Reference.Equals(other.Reference))
                && Pattern == other.Pattern
                && MaxLength == other.MaxLength
                && MinLength == other.MinLength
                && MultipleOf == other.MultipleOf
                && Maximum == other.Maximum
                && ExclusiveMaximum == other.ExclusiveMaximum
                && MinItems ==  other.MinItems
                && MaxItems == other.MaxItems
                && UniqueItems == other.UniqueItems
                && string.Equals(Format, other.Format, StringComparison.Ordinal)
                && (AllOf == null
                        ? other.AllOf == null
                        : AllOf.HasSameElementsAs(other.AllOf))
                && (AnyOf == null
                        ? other.AnyOf == null
                        : AnyOf.HasSameElementsAs(other.AnyOf))
                && (OneOf == null
                        ? other.OneOf == null
                        : OneOf.HasSameElementsAs(other.OneOf))
                && (Not == null
                        ? other.Not == null
                        : Not.Equals(other.Not));
        }

        #endregion

        public static bool operator ==(JsonSchema left, JsonSchema right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((object)left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(JsonSchema left, JsonSchema right)
        {
            return !(left == right);
        }
    }
}
