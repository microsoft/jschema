// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.Validation
{
    internal class JTokenEqualityComparer : IEqualityComparer<JToken>
    {
        internal static readonly JTokenEqualityComparer Instance = new JTokenEqualityComparer();

        public bool Equals(JToken x, JToken y)
        {
            return DeepEquals(x, y);
        }

        public int GetHashCode(JToken obj)
        {
            return DeepHashCode(obj);
        }

        internal static bool DeepEquals(JToken jToken, object obj)
        {
            switch (jToken.Type)
            {
                case JTokenType.String:
                    return ValueEquals<string>(jToken, obj);

                case JTokenType.Integer:
                    return ValueEquals<long>(jToken, obj);

                case JTokenType.Float:
                    return ValueEquals<double>(jToken, obj);

                case JTokenType.Boolean:
                    return ValueEquals<bool>(jToken, obj);

                case JTokenType.Array:
                    return ArrayEquals(jToken as JArray, obj);

                case JTokenType.Object:
                    return ObjectEquals(jToken as JObject, obj);

                case JTokenType.Null:
                    return NullEquals(obj);

                default:
                    return false;
            }
        }

        internal static int DeepHashCode(JToken jToken)
        {
            unchecked
            {
                switch (jToken.Type)
                {
                    case JTokenType.String:
                    case JTokenType.Boolean:
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        return jToken.Value<object>().GetHashCode();

                    case JTokenType.Null:
                        return jToken.Type.GetHashCode();

                    case JTokenType.Array:
                        return ArrayHashCode(jToken as JArray);

                    case JTokenType.Object:
                        return ObjectHashCode(jToken as JObject);

                    default:
                        return 0;
                }
            }
        }

        private static bool ValueEquals<T>(JToken jToken, object obj)
        {
            T value;
            JToken objToken = obj as JToken;
            if (objToken != null && objToken.Type == jToken.Type)
            {
                value = objToken.Value<T>();
            }
            else if (obj is T)
            {
                value = (T)obj;
            }
            else
            {
                return false;
            }

            return Equals(value, jToken.Value<T>());
        }

        private static bool ArrayEquals(JArray jArray, object obj)
        {
            JArray objJArray = obj as JArray;
            if (objJArray == null || objJArray.Count != jArray.Count)
            {
                return false;
            }

            JToken[] tokens = jArray.ToArray();
            JToken[] objTokens = objJArray.ToArray();
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (!DeepEquals(tokens[i], objTokens[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ObjectEquals(JObject jObject, object obj)
        {
            JObject objJObject = obj as JObject;
            if (objJObject == null)
            {
                return false;
            }

            IList<string> propertyNames = GetPropertyNames(jObject);
            IList<string> objPropertyNames = GetPropertyNames(objJObject);

            if (!propertyNames.HasSameElementsAs(objPropertyNames))
            {
                return false;
            }

            return propertyNames.All(pn => DeepEquals(jObject[pn], objJObject[pn]));
        }

        private static bool NullEquals(object obj)
        {
            if (obj == null)
            {
                return true;
            }

            if (obj is JToken)
            {
                return (obj as JToken).Type == JTokenType.Null;
            }

            return false;
        }

        private const int GetHashCodeSeedValue = 17;
        private const int GetHashCodeCombiningValue = 31;

        private static int ObjectHashCode(JObject jObject)
        {
            int result = GetHashCodeSeedValue;

            foreach (string propertyName in GetPropertyNames(jObject))
            {
                result = (result * GetHashCodeCombiningValue) + propertyName.GetHashCode();
                result = (result * GetHashCodeCombiningValue) + DeepHashCode(jObject[propertyName]);
            }

            return result;
        }

        private static int ArrayHashCode(JArray jArray)
        {
            int result = GetHashCodeSeedValue;

            foreach (JToken jToken in jArray)
            {
                result = (result * GetHashCodeCombiningValue) + DeepHashCode(jToken);
            }

            return result;
        }

        private static List<string> GetPropertyNames(JObject jObject)
        {
            return jObject.Properties().Select(p => p.Name).ToList();
        }
    }
}
