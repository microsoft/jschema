// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace Microsoft.Json.Pointer.UnitTests
{
    public class EvaluationTestCase : IXunitSerializable
    {
        public EvaluationTestCase(
            string name,
            string document,
            string pointer,
            bool valid,
            string result = null)
        {
            Name = name;
            Document = document;
            Pointer = pointer;
            Valid = valid;
            Result = result;
        }

        public EvaluationTestCase()
        {
            // Needed for deserialization
        }

        public string Name { get; private set; }
        public string Document { get; private set; }
        public string Pointer { get; private set; }
        public bool Valid { get; private set; }
        public string Result { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Document = info.GetValue<string>(nameof(Document));
            Pointer = info.GetValue<string>(nameof(Pointer));
            Valid = info.GetValue<bool>(nameof(Valid));
            Result = info.GetValue<string>(nameof(Result));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Document), Document);
            info.AddValue(nameof(Pointer), Pointer);
            info.AddValue(nameof(Valid), Valid);
            info.AddValue(nameof(Result), Result);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
