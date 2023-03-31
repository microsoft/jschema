// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Represents a dictionary that maps from the URI of a schema to an array of code
    /// generation hints that apply to that schema.
    /// </summary>
    public class HintDictionary : Dictionary<string, CodeGenHint[]>
    {
        private const string WildCard = "*";
        private readonly string _typeNameSuffix;

        /// <summary>
        /// Dictionary that maps each kind of code generation hint to a method that
        /// instantiates it from a set of arguments.
        /// </summary>
        private readonly IDictionary<HintKind, HintCreator> _hintCreatorDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="HintDictionary"/> class.
        /// </summary>
        public HintDictionary(string dictionaryText, string typeNameSuffix = null)
        {
            _typeNameSuffix = string.IsNullOrWhiteSpace(typeNameSuffix)
                ? string.Empty
                : typeNameSuffix.Trim();

            _hintCreatorDictionary = ImmutableDictionary.CreateRange(
                new Dictionary<HintKind, HintCreator>
                {
                    [HintKind.AttributeHint] = CreateAttributeHint,
                    [HintKind.BaseTypeHint] = CreateBaseTypeNameHint,
                    [HintKind.ClassNameHint] = CreateClassNameHint,
                    [HintKind.DictionaryHint] = CreateDictionaryHint,
                    [HintKind.EnumHint] = CreateEnumHint,
                    [HintKind.InterfaceHint] = CreateInterfaceHint,
                    [HintKind.PropertyHint] = CreatePropertyHint
                });

        var infoDictionary = HintInstantiationInfoDictionary.Deserialize(dictionaryText);
            InstantiateHints(infoDictionary);
        }

        public T GetHint<T>(string key) where T : CodeGenHint
        {
            T[] hints = GetHints<T>(key);
            return hints?.Length > 0 ? hints[0] : null;
        }

        public T[] GetHints<T>(string key) where T : CodeGenHint
        {
            CodeGenHint[] allHints = null;
            T[] hints = null;

            if (TryGetValue(key, out allHints))
            {
                hints = allHints.Where(h => h is T).Cast<T>().ToArray();
            }

            return hints;
        }

        public T GetPropertyHint<T>(string typeName, string propertyName)
            where T: CodeGenHint
        {
            string key = MakeHintDictionaryKey(typeName, propertyName);
            T hint = GetHint<T>(key);
            if (hint == null)
            {
                key = MakeHintDictionaryKey(WildCard, propertyName);
                hint = GetHint<T>(key);
            }

            return hint;
        }

        private static string MakeHintDictionaryKey(string typeName, string propertyName)
        {
            return typeName + "." + propertyName.ToPascalCase();
        }

        private void InstantiateHints(HintInstantiationInfoDictionary infoDictionary)
        {
            foreach (string key in infoDictionary.Keys)
            {
                var hints = new List<CodeGenHint>();
                foreach (HintInstantiationInfo info in infoDictionary[key])
                {
                    CodeGenHint hint = CreateHintFromInfo(info);
                    if (hint == null)
                    {
                        throw new ApplicationException(
                            $"Cannot create a code generation hint of unrecognized kind '{info.Kind}'.");
                    }

                    hints.Add(hint);
                }

                this[key] = hints.ToArray();
            }
        }

        /// <summary>
        /// Delegate that creates a code generation hint from a list of arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments used to instantiate the code generation hint. Can be null
        /// if the type of hint being instantiated does not required arguments.
        /// </param>
        /// <returns>
        /// An instantiated code generation hint.
        /// </returns>
        private delegate CodeGenHint HintCreator(JObject arguments);
        private CodeGenHint CreateHintFromInfo(HintInstantiationInfo info)
        {
            CodeGenHint hint = null;

            HintCreator hintCreator;
            if (_hintCreatorDictionary.TryGetValue(info.Kind, out hintCreator))
            {
                hint = hintCreator(info.Arguments);
            }

            return hint;
        }

        private static CodeGenHint CreateAttributeHint(JObject arguments)
        {
            string typeName = GetArgument<string>(arguments, nameof(AttributeHint.TypeName));
            string namespaceName = GetArgument<string>(arguments, nameof(AttributeHint.NamespaceName));
            string[] attributeArguments = GetArrayArgument<string>(arguments, nameof(AttributeHint.Arguments));
            IDictionary<string, string> attributeProperties = GetObjectArgument(arguments, nameof(AttributeHint.Properties));

            return new AttributeHint(typeName, namespaceName, attributeArguments, attributeProperties);
        }

        private CodeGenHint CreateBaseTypeNameHint(JObject arguments)
        {

            string[] baseTypeNames = GetArrayArgument<string>(arguments, nameof(BaseTypeHint.BaseTypeNames))
                .Select(btn => btn + _typeNameSuffix)
                .ToArray();

            return new BaseTypeHint(baseTypeNames);
        }

        private static CodeGenHint CreateClassNameHint(JObject arguments)
        {
            string className = GetArgument<string>(arguments, nameof(ClassNameHint.ClassName));

            return new ClassNameHint(className);
        }

        private static CodeGenHint CreateDictionaryHint(JObject arguments)
        {
            string keyTypeName = GetArgument<string>(arguments, nameof(DictionaryHint.KeyTypeName));
            string valueTypeName = GetArgument<string>(arguments, nameof(DictionaryHint.ValueTypeName));
            string namespaceName = GetArgument<string>(arguments, nameof(DictionaryHint.NamespaceName));
            string comparisonKind = GetArgument<string>(arguments, nameof(DictionaryHint.ComparisonKind));
            string hashKind = GetArgument<string>(arguments, nameof(DictionaryHint.HashKind));
            string initializationKind = GetArgument<string>(arguments, nameof(DictionaryHint.InitializationKind));

            return new DictionaryHint(keyTypeName, valueTypeName, namespaceName, comparisonKind, hashKind, initializationKind);
        }

        private static CodeGenHint CreateEnumHint(JObject arguments)
        {
            string typeName = GetArgument<string>(arguments, nameof(EnumHint.TypeName));
            string description = GetArgument<string>(arguments, nameof(EnumHint.Description));
            string[] memberNames = GetArrayArgument<string>(arguments, nameof(EnumHint.MemberNames));
            int[] memberValues = GetArrayArgument<int>(arguments, nameof(EnumHint.MemberValues));
            string zeroValue = GetArgument<string>(arguments, nameof(EnumHint.ZeroValueName));
            bool flags = GetArgument<bool>(arguments, nameof(EnumHint.Flags));
            bool allowMemberCountMismatch = GetArgument<bool>(arguments, nameof(EnumHint.AllowMemberCountMismatch));

            return new EnumHint(typeName, description, memberNames, memberValues, zeroValue, flags, allowMemberCountMismatch);
        }

        private static CodeGenHint CreateInterfaceHint(JObject arguments)
        {
            return new InterfaceHint
            {
                Description = GetArgument<string>(arguments, nameof(InterfaceHint.Description))
            };
        }

        private static CodeGenHint CreatePropertyHint(JObject arguments)
        {
            string[] modifiers = GetArrayArgument<string>(arguments, nameof(PropertyHint.Modifiers));
            string typeName = GetArgument<string>(arguments, nameof(PropertyHint.TypeName));
            string name = GetArgument<string>(arguments, nameof(PropertyHint.Name));

            return new PropertyHint(modifiers, typeName, name);
        }

        private static T GetArgument<T>(JObject arguments, string name)
        {
            return arguments == null
                ? default(T)
                : arguments.Value<T>(name.ToCamelCase());
        }

        private static T[] GetArrayArgument<T>(JObject arguments, string name)
        {
            if (arguments == null)
            {
                return default(T[]);
            }

            JArray arrayValue = arguments.Value<JArray>(name.ToCamelCase());
            if (arrayValue == null)
            {
                return default(T[]);
            }
            
            return arrayValue.Values<T>().ToArray();
        }

        private static IDictionary<string, string> GetObjectArgument(JObject arguments, string name)
        {
            JObject jObject = arguments?.Value<JObject>(name.ToCamelCase());
            if (jObject == null)
            {
                return null;
            }

            var dictionary = new Dictionary<string, string>();
            foreach (JProperty jProperty in jObject.Properties())
            {
                dictionary.Add(jProperty.Name, jProperty.Value.ToString());
            }

            return dictionary;
        }
    }
}
