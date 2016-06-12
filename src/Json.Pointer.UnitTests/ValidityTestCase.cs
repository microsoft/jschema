// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Json.Pointer.UnitTests
{
    public class ValidityTestCase : IXunitSerializable
    {
        public ValidityTestCase(
            string name)
        {
            Name = name;
        }

        public ValidityTestCase()
        {
            // Needed for deserialization
        }

        public string Name { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
