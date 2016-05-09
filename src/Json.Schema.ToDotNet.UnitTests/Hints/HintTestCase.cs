// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit.Abstractions;

public class HintTestCase : IXunitSerializable
{
    public HintTestCase(
        string name,
        string schemaText,
        string hintsText,
        string expectedOutput,
        bool shouldThrow = false,
        string expectedErrorMessage = null)
    {
        Name = name;
        SchemaText = schemaText;
        HintsText = hintsText;
        ExpectedOutput = expectedOutput;
        ShouldThrow = shouldThrow;
        ExpectedErrorMessage = expectedErrorMessage;
    }

    public HintTestCase()
    {
        // Required for deserialization.
    }

    public string Name;
    public string SchemaText;
    public string HintsText;
    public string ExpectedOutput;
    public bool ShouldThrow;
    public string ExpectedErrorMessage;

    public void Deserialize(IXunitSerializationInfo info)
    {
        Name = info.GetValue<string>(nameof(Name));
        SchemaText = info.GetValue<string>(nameof(SchemaText));
        HintsText = info.GetValue<string>(nameof(HintsText));
        ExpectedOutput = info.GetValue<string>(nameof(ExpectedOutput));
        ShouldThrow = info.GetValue<bool>(nameof(ShouldThrow));
        ExpectedErrorMessage = info.GetValue<string>(nameof(ExpectedErrorMessage));
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(SchemaText), SchemaText);
        info.AddValue(nameof(HintsText), HintsText);
        info.AddValue(nameof(ExpectedOutput), ExpectedOutput);
        info.AddValue(nameof(ShouldThrow), ShouldThrow);
        info.AddValue(nameof(ExpectedErrorMessage), ExpectedErrorMessage);
    }

    public override string ToString()
    {
        return Name;
    }
}
