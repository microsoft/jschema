// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class ValidityTestCase : IXunitSerializable
    {
        public ValidityTestCase(
            string name,
            string value,
            bool valid)
        {
            Name = name;
            Value = value;
            Valid = valid;
        }

        public ValidityTestCase()
        {
            // Needed for deserialization
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
        public bool Valid { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Value = info.GetValue<string>(nameof(Value));
            Valid = info.GetValue<bool>(nameof(Valid));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Value), Value);
            info.AddValue(nameof(Valid), Valid);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
