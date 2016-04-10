// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.Json.Schema.UnitTests
{
    public class EqualityTestCase : IXunitSerializable
    {
        public EqualityTestCase(
            string name,
            string left,
            string right,
            bool shouldBeEqual)
        {
            Name = name;
            Left = left;
            Right = right;
            ShouldBeEqual = shouldBeEqual;
        }

        public EqualityTestCase()
        {
            // Needed for deserialization.
        }

        public string Name;
        public string Left;
        public string Right;
        public bool ShouldBeEqual;

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Left = info.GetValue<string>(nameof(Left));
            Right = info.GetValue<string>(nameof(Right));
            ShouldBeEqual = info.GetValue<bool>(nameof(ShouldBeEqual));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Left), Left);
            info.AddValue(nameof(Right), Right);
            info.AddValue(nameof(ShouldBeEqual), ShouldBeEqual);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
