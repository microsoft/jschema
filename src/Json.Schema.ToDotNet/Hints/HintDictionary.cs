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
        /// <summary>
        /// Initializes a new instance of the <see cref="HintDictionary"/> class.
        /// </summary>
        public HintDictionary(string dictionaryText)
        {
            var infoDictionary = HintInstantiationInfoDictionary.Deserialize(dictionaryText);
            InstantiateHints(infoDictionary);
        }

        public T GetHint<T>(string key) where T : CodeGenHint
        {
            T hint = null;

            CodeGenHint[] hints;
            if (TryGetValue(key, out hints))
            {
                hint = hints.FirstOrDefault(h => h is T) as T;
            }

            return hint;
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

        /// <summary>
        /// Dictionary that maps each kind of code generation hint to a method that
        /// instantiates it from a set of arguments.
        /// </summary>
        private static readonly IDictionary<HintKind, HintCreator> s_hintCreatorDictionary = ImmutableDictionary.CreateRange(
            new Dictionary<HintKind, HintCreator>
            {
                [HintKind.AttributeHint] = CreateAttributeHint,
                [HintKind.ClassNameHint] = CreateClassNameHint,
                [HintKind.DictionaryHint] = CreateDictionaryHint,
                [HintKind.EnumHint] = CreateEnumHint,
                [HintKind.InterfaceHint] = CreateInterfaceHint
            });

        private CodeGenHint CreateHintFromInfo(HintInstantiationInfo info)
        {
            CodeGenHint hint = null;

            HintCreator hintCreator;
            if (s_hintCreatorDictionary.TryGetValue(info.Kind, out hintCreator))
            {
                hint = hintCreator(info.Arguments);
            }

            return hint;
        }

        private static CodeGenHint CreateAttributeHint(JObject arguments)
        {
            string attributeTypeName = GetArgument<string>(arguments, nameof(AttributeHint.AttributeTypeName));

            return new AttributeHint(attributeTypeName);
        }

        private static CodeGenHint CreateClassNameHint(JObject arguments)
        {
            string className = GetArgument<string>(arguments, nameof(ClassNameHint.ClassName));

            return new ClassNameHint(className);
        }

        private static CodeGenHint CreateDictionaryHint(JObject arguments)
        {
            string keyTypeName = GetArgument<string>(arguments, nameof(DictionaryHint.KeyTypeName));

            return new DictionaryHint(keyTypeName);
        }

        private static CodeGenHint CreateEnumHint(JObject arguments)
        {
            string typeName = GetArgument<string>(arguments, nameof(EnumHint.TypeName));
            string description = GetArgument<string>(arguments, nameof(EnumHint.Description));
            string[] enumValues = GetArrayArgument<string>(arguments, nameof(EnumHint.EnumValues));
            string zeroValue = GetArgument<string>(arguments, nameof(EnumHint.ZeroValue));

            return new EnumHint(typeName, description, enumValues, zeroValue);
        }

        private static CodeGenHint CreateInterfaceHint(JObject arguments)
        {
            return new InterfaceHint
            {
                Description = GetArgument<string>(arguments, nameof(InterfaceHint.Description))
            };
        }

        private static T GetArgument<T>(JObject arguments, string dotNetPropertyName)
        {
            return arguments == null
                ? default(T)
                : arguments.Value<T>(dotNetPropertyName.ToCamelCase());
        }

        private static T[] GetArrayArgument<T>(JObject arguments, string dotNetPropertyName)
        {
            if (arguments == null)
            {
                return default(T[]);
            }

            JArray arrayValue = arguments.Value<JArray>(dotNetPropertyName.ToCamelCase());
            if (arrayValue == null)
            {
                return default(T[]);
            }
            
            return arrayValue.Values<T>().ToArray();
        }
    }
}
