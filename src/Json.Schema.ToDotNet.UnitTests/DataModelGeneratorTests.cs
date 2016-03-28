// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Json.Schema.UnitTests;
using Xunit;

namespace Microsoft.Json.Schema.ToDotNet.UnitTests
{
    public class DataModelGeneratorTests
    {
        private const string PrimaryOutputFilePath = TestFileSystem.OutputDirectory + "\\" + TestSettings.RootClassName + ".cs";

        private TestFileSystem _testFileSystem;
        private readonly DataModelGeneratorSettings _settings;

        public DataModelGeneratorTests()
        {
            _testFileSystem = new TestFileSystem();
            _settings = TestSettings.MakeSettings();
        }

        [Fact(DisplayName = "DataModelGenerator throws if output directory exists")]
        public void ThrowsIfOutputDirectoryExists()
        {
            _settings.ForceOverwrite = false;

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(new JsonSchema());

            // ... and the message should mention the output directory.
            action.ShouldThrow<JSchemaException>().WithMessage($"*{TestFileSystem.OutputDirectory}*");
        }

        [Fact(DisplayName = "DataModelGenerator does not throw if output directory does not exist")]
        public void DoesNotThrowIfOutputDirectoryDoesNotExist()
        {
            // Use a directory name other than the default. The mock file system believes
            // that only the default directory exists.
            _settings.OutputDirectory = _settings.OutputDirectory + "x";

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldNotThrow();
        }

        [Fact(DisplayName = "DataModelGenerator does not throw if ForceOverwrite setting is set")]
        public void DoesNotThowIfForceOverwriteSettingIsSet()
        {
            // This is the default from MakeSettings; restated here for explicitness.
            _settings.ForceOverwrite = true;

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldNotThrow();
        }

        [Fact(DisplayName = "DataModelGenerator throws if root schema is not of type 'object'")]
        public void ThrowsIfRootSchemaIsNotOfTypeObject()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("NotAnObject");

            Action action = () => generator.Generate(schema);

            // ... and the message should mention what the root type actually was.
            action.ShouldThrow<JSchemaException>().WithMessage("*number*");
        }

        [Fact(DisplayName = "DataModelGenerator generates class description")]
        public void GeneratesClassDescription()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Basic");

            string actual = generator.Generate(schema);

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// The description
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
    }
}";
            actual.Should().Be(Expected);

            // This particular test shows not only that the correct text was produced,
            // but that it was written to the expected path. Subsequent tests will
            // just verify the text.
            _testFileSystem.Files.Should().OnlyContain(key => key.Equals(PrimaryOutputFilePath));
        }

        [Fact(DisplayName = "DataModelGenerator generates properties with built-in types")]
        public void GeneratesPropertiesWithBuiltInTypes()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Properties");

            string actual = generator.Generate(schema);

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""stringProp"", IsRequired = false, EmitDefaultValue = false)]
        public string StringProp { get; set; }
        [DataMember(Name = ""numberProp"", IsRequired = false, EmitDefaultValue = false)]
        public double NumberProp { get; set; }
        [DataMember(Name = ""booleanProp"", IsRequired = false, EmitDefaultValue = false)]
        public bool BooleanProp { get; set; }
        [DataMember(Name = ""integerProp"", IsRequired = false, EmitDefaultValue = false)]
        public int IntegerProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (StringProp != null)
                {
                    result = (result * 31) + StringProp.GetHashCode();
                }

                result = (result * 31) + NumberProp.GetHashCode();
                result = (result * 31) + BooleanProp.GetHashCode();
                result = (result * 31) + IntegerProp.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (StringProp != other.StringProp)
            {
                return false;
            }

            if (NumberProp != other.NumberProp)
            {
                return false;
            }

            if (BooleanProp != other.BooleanProp)
            {
                return false;
            }

            if (IntegerProp != other.IntegerProp)
            {
                return false;
            }

            return true;
        }
    }
}";
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates object-valued property with correct type")]
        public void GeneratesObjectValuedPropertyWithCorrectType()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Object");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""objectProp"", IsRequired = false, EmitDefaultValue = false)]
        public D ObjectProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (ObjectProp != null)
                {
                    result = (result * 31) + ObjectProp.GetHashCode();
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.Equals(ObjectProp, other.ObjectProp))
            {
                return false;
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference is not a fragment")]
        public void ThrowsIfReferenceIsNotAFragment()
        {
            const string SchemaText = @"
{
  ""type"": ""object"",
  ""properties"": {
    ""p"": {
      ""$ref"": ""https://example.com/pschema.schema.json/#""
    }
  },
}";
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldThrow<JSchemaException>()
                .WithMessage("*https://example.com/pschema.schema.json/#*");
        }

        [Fact(DisplayName = "DataModelGenerator throws if reference does not specify a definition")]
        public void ThrowsIfReferenceDoesNotSpecifyADefinition()
        {
            const string SchemaText = @"
{
  ""type"": ""object"",
  ""properties"": {
    ""p"": {
      ""$ref"": ""#/notDefinitions/p""
    }
  },
  ""notDefinitions"": {
    ""p"": {
    }
  }
}";
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldThrow<JSchemaException>()
                .WithMessage("*#/notDefinitions/p*");
        }

        [Fact(DisplayName = "Throws if referenced definition does not exist")]
        public void ThrowsIfReferencedDefinitionDoesNotExist()
        {
            const string SchemaText = @"
{
  ""type"": ""object"",
  ""properties"": {
    ""p"": {
      ""$ref"": ""#/definitions/nonExistentDefinition""
    }
  },
  ""definitions"": {
    ""p"": {
    }
  }
}";
            JsonSchema schema = SchemaReader.ReadSchema(SchemaText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            Action action = () => generator.Generate(schema);

            action.ShouldThrow<JSchemaException>()
                .WithMessage("*nonExistentDefinition*");
        }

        [Fact(DisplayName = "DataModelGenerator generates array-valued property")]
        public void GeneratesArrayValuedProperty()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Array");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""arrayProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<object> ArrayProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (ArrayProp != null)
                {
                    foreach (var value_0 in ArrayProp)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(ArrayProp, other.ArrayProp))
            {
                if (ArrayProp == null || other.ArrayProp == null)
                {
                    return false;
                }

                if (ArrayProp.Count != other.ArrayProp.Count)
                {
                    return false;
                }

                for (int value_0 = 0; value_0 < ArrayProp.Count; ++value_0)
                {
                    if (!Object.Equals(ArrayProp[value_0], other.ArrayProp[value_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(Expected, actual, nameof(GeneratesArrayValuedProperty));

            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties")]
        public void GeneratesXmlCommentsForProperties()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// An example property.
        /// </summary>
        [DataMember(Name = ""exampleProp"", IsRequired = false, EmitDefaultValue = false)]
        public string ExampleProp { get; set; }
    }
}";

            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates XML comments for properties whose property type is ref")]
        public void GeneratesXmlCommentsForPropertiesWhosePropertyTypeIsRef()
        {
            _settings.RootClassName = "consoleWindow";
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""description"": ""Describes a console window."",
  ""properties"": {
    ""foregroundColor"": {
      ""$ref"": ""#/definitions/color"",
      ""description"": ""The color of the text on the screen.""
    },
    ""backgroundColor"": {
      ""$ref"": ""#/definitions/color"",
      ""description"": ""The color of the screen background.""
    },
  },
  ""definitions"": {
    ""color"": {
      ""type"": ""object"",
      ""description"": ""Describes a color with R, G, and B components."",
      ""properties"": {
        ""red"": {
          ""type"": ""integer"",
          ""description"": ""The value of the R component.""
        },
        ""green"": {
          ""type"": ""integer"",
          ""description"": ""The value of the G component.""
        },
        ""blue"": {
          ""type"": ""integer"",
          ""description"": ""The value of the B component.""
        }
      }
    }
  }
}");

            const string RootClassText =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// Describes a console window.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class ConsoleWindow : IEquatable<ConsoleWindow>
    {
        /// <summary>
        /// The color of the text on the screen.
        /// </summary>
        [DataMember(Name = ""foregroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color ForegroundColor { get; set; }

        /// <summary>
        /// The color of the screen background.
        /// </summary>
        [DataMember(Name = ""backgroundColor"", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}";

            const string ColorClassText =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// Describes a color with R, G, and B components.
    /// </summary>
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class Color : IEquatable<Color>
    {
        /// <summary>
        /// The value of the R component.
        /// </summary>
        [DataMember(Name = ""red"", IsRequired = false, EmitDefaultValue = false)]
        public int Red { get; set; }

        /// <summary>
        /// The value of the G component.
        /// </summary>
        [DataMember(Name = ""green"", IsRequired = false, EmitDefaultValue = false)]
        public int Green { get; set; }

        /// <summary>
        /// The value of the B component.
        /// </summary>
        [DataMember(Name = ""blue"", IsRequired = false, EmitDefaultValue = false)]
        public int Blue { get; set; }
    }
}";

            generator.Generate(schema);

            string rootFilePath = TestFileSystem.MakeOutputFilePath(_settings.RootClassName.ToPascalCase());
            string colorFilePath = TestFileSystem.MakeOutputFilePath("Color");

            var expectedOutputFiles = new List<string>
            {
                rootFilePath,
                colorFilePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(key => expectedOutputFiles.Contains(key));

            _testFileSystem[rootFilePath].Should().Be(RootClassText);
            _testFileSystem[colorFilePath].Should().Be(ColorClassText);
        }

        [Fact(DisplayName = "DataModelGenerator generates copyright notice")]
        public void GeneratesCopyrightAtTopOfFile()
        {
            _settings.CopyrightNotice =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.

";
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("PropertyDescription");

            const string Expected =
@"// Copyright (c) 2016. All rights reserved.
// Licensed under Apache 2.0 license.

using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        /// <summary>
        /// An example property.
        /// </summary>
        [DataMember(Name = ""exampleProp"", IsRequired = false, EmitDefaultValue = false)]
        public string ExampleProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates cloning code")]
        public void GeneratesCloningCode()
        {
            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""intProp"": {
      ""type"": ""integer"",
      ""description"": ""An integer property.""
    },
    ""stringProp"": {
      ""type"": ""string"",
      ""description"": ""A string property.""
    },
    ""arrayProp"": {
      ""type"": ""array"",
      ""description"": ""An array property."",
      ""items"": {
        ""type"": ""number""
      }
    },
    ""uriProp"": {
      ""type"": ""string"",
      ""description"": ""A Uri property."",
      ""format"": ""uri""
    },
    ""dateTimeProp"": {
      ""type"": ""string"",
      ""description"": ""A DateTime property."",
      ""format"": ""date-time""
    },
    ""referencedTypeProp"": {
      ""$ref"": ""#/definitions/d""
    },
    ""arrayOfRefProp"": {
      ""type"": ""array"",
      ""description"": ""An array of a cloneable type."",
      ""items"": {
        ""$ref"": ""#/definitions/d""
      }
    },
    ""arrayOfArrayProp"": {
      ""type"": ""array"",
      ""description"": ""An array of arrays."",
      ""items"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/definitions/d""
        }
      }
    },
    ""dictionaryProp"": {
      ""description"": ""A dictionary property."",
      ""type"": ""object""
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""object""
    }
  }
}");

            const string HintsText =
@"{
  ""C.DictionaryProp"": [
    {
      ""$type"": ""Microsoft.Json.Schema.ToDotNet.DictionaryHint, Microsoft.Json.Schema.ToDotNet""
    }
  ]
}";

            const string ExpectedClass =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : ISNode, IEquatable<C>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        public SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

        /// <summary>
        /// An integer property.
        /// </summary>
        [DataMember(Name = ""intProp"", IsRequired = false, EmitDefaultValue = false)]
        public int IntProp { get; set; }

        /// <summary>
        /// A string property.
        /// </summary>
        [DataMember(Name = ""stringProp"", IsRequired = false, EmitDefaultValue = false)]
        public string StringProp { get; set; }

        /// <summary>
        /// An array property.
        /// </summary>
        [DataMember(Name = ""arrayProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<double> ArrayProp { get; set; }

        /// <summary>
        /// A Uri property.
        /// </summary>
        [DataMember(Name = ""uriProp"", IsRequired = false, EmitDefaultValue = false)]
        public Uri UriProp { get; set; }

        /// <summary>
        /// A DateTime property.
        /// </summary>
        [DataMember(Name = ""dateTimeProp"", IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTimeProp { get; set; }
        [DataMember(Name = ""referencedTypeProp"", IsRequired = false, EmitDefaultValue = false)]
        public D ReferencedTypeProp { get; set; }

        /// <summary>
        /// An array of a cloneable type.
        /// </summary>
        [DataMember(Name = ""arrayOfRefProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<D> ArrayOfRefProp { get; set; }

        /// <summary>
        /// An array of arrays.
        /// </summary>
        [DataMember(Name = ""arrayOfArrayProp"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<D>> ArrayOfArrayProp { get; set; }

        /// <summary>
        /// A dictionary property.
        /// </summary>
        [DataMember(Name = ""dictionaryProp"", IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, string> DictionaryProp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class.
        /// </summary>
        public C()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the supplied values.
        /// </summary>
        /// <param name=""intProp"">
        /// An initialization value for the <see cref=""P: IntProp"" /> property.
        /// </param>
        /// <param name=""stringProp"">
        /// An initialization value for the <see cref=""P: StringProp"" /> property.
        /// </param>
        /// <param name=""arrayProp"">
        /// An initialization value for the <see cref=""P: ArrayProp"" /> property.
        /// </param>
        /// <param name=""uriProp"">
        /// An initialization value for the <see cref=""P: UriProp"" /> property.
        /// </param>
        /// <param name=""dateTimeProp"">
        /// An initialization value for the <see cref=""P: DateTimeProp"" /> property.
        /// </param>
        /// <param name=""referencedTypeProp"">
        /// An initialization value for the <see cref=""P: ReferencedTypeProp"" /> property.
        /// </param>
        /// <param name=""arrayOfRefProp"">
        /// An initialization value for the <see cref=""P: ArrayOfRefProp"" /> property.
        /// </param>
        /// <param name=""arrayOfArrayProp"">
        /// An initialization value for the <see cref=""P: ArrayOfArrayProp"" /> property.
        /// </param>
        /// <param name=""dictionaryProp"">
        /// An initialization value for the <see cref=""P: DictionaryProp"" /> property.
        /// </param>
        public C(int intProp, string stringProp, IEnumerable<double> arrayProp, Uri uriProp, DateTime dateTimeProp, D referencedTypeProp, IEnumerable<D> arrayOfRefProp, IEnumerable<IEnumerable<D>> arrayOfArrayProp, Dictionary<string, string> dictionaryProp)
        {
            Init(intProp, stringProp, arrayProp, uriProp, dateTimeProp, referencedTypeProp, arrayOfRefProp, arrayOfArrayProp, dictionaryProp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""C"" /> class from the specified instance.
        /// </summary>
        /// <param name=""other"">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref=""ArgumentNullException"">
        /// Thrown if <paramref name=""other"" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.IntProp, other.StringProp, other.ArrayProp, other.UriProp, other.DateTimeProp, other.ReferencedTypeProp, other.ArrayOfRefProp, other.ArrayOfArrayProp, other.DictionaryProp);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        private void Init(int intProp, string stringProp, IEnumerable<double> arrayProp, Uri uriProp, DateTime dateTimeProp, D referencedTypeProp, IEnumerable<D> arrayOfRefProp, IEnumerable<IEnumerable<D>> arrayOfArrayProp, Dictionary<string, string> dictionaryProp)
        {
            IntProp = intProp;
            StringProp = stringProp;
            if (arrayProp != null)
            {
                var destination_0 = new List<double>();
                foreach (var value_0 in arrayProp)
                {
                    destination_0.Add(value_0);
                }

                ArrayProp = destination_0;
            }

            if (uriProp != null)
            {
                UriProp = new Uri(uriProp.OriginalString, uriProp.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            DateTimeProp = dateTimeProp;
            if (referencedTypeProp != null)
            {
                ReferencedTypeProp = new D(referencedTypeProp);
            }

            if (arrayOfRefProp != null)
            {
                var destination_1 = new List<D>();
                foreach (var value_1 in arrayOfRefProp)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new D(value_1));
                    }
                }

                ArrayOfRefProp = destination_1;
            }

            if (arrayOfArrayProp != null)
            {
                var destination_2 = new List<IList<D>>();
                foreach (var value_2 in arrayOfArrayProp)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        var destination_3 = new List<D>();
                        foreach (var value_3 in value_2)
                        {
                            if (value_3 == null)
                            {
                                destination_3.Add(null);
                            }
                            else
                            {
                                destination_3.Add(new D(value_3));
                            }
                        }

                        destination_2.Add(destination_3);
                    }
                }

                ArrayOfArrayProp = destination_2;
            }

            if (dictionaryProp != null)
            {
                DictionaryProp = new Dictionary<string, string>(dictionaryProp);
            }
        }
    }
}";
            const string ExpectedSyntaxInterface =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// An interface for all types generated from the S schema.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public interface ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref=""ISNode"" />.
        /// </summary>
        SNodeKind SNodeKind { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISNode DeepClone();
    }
}";
            const string ExpectedKindEnum =
@"using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// A set of values for all the types that implement <see cref=""ISNode"" />.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public enum SNodeKind
    {
        /// <summary>
        /// An uninitialized kind.
        /// </summary>
        None,
        /// <summary>
        /// A value indicating that the <see cref=""ISNode"" /> object is of type <see cref=""C"" />.
        /// </summary>
        C,
        /// <summary>
        /// A value indicating that the <see cref=""ISNode"" /> object is of type <see cref=""D"" />.
        /// </summary>
        D
    }
}";
            const string ExpectedRewritingVisitor =
@"using System;
using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// Rewriting visitor for the S object model.
    /// </summary>
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public abstract class SRewritingVisitor
    {
        /// <summary>
        /// Starts a rewriting visit of a node in the S object model.
        /// </summary>
        /// <param name=""node"">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object Visit(ISNode node)
        {
            return this.VisitActual(node);
        }
    }
}";
            _settings.GenerateCloningCode = true;
            _settings.HintDictionary = HintDictionary.Deserialize(HintsText);
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            generator.Generate(schema);

            string syntaxInterfacePath = TestFileSystem.MakeOutputFilePath("ISNode");
            string kindEnumPath = TestFileSystem.MakeOutputFilePath("SNodeKind");
            string referencedTypePath = TestFileSystem.MakeOutputFilePath("D");
            string rewritingVisitorClassPath = TestFileSystem.MakeOutputFilePath("SRewritingVisitor");

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                syntaxInterfacePath,
                kindEnumPath,
                rewritingVisitorClassPath,
                referencedTypePath
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(ExpectedClass);
            _testFileSystem[syntaxInterfacePath].Should().Be(ExpectedSyntaxInterface);
            _testFileSystem[kindEnumPath].Should().Be(ExpectedKindEnum);
            _testFileSystem[rewritingVisitorClassPath].Should().Be(ExpectedRewritingVisitor);
        }

        [Fact(DisplayName = "DataModelGenerator generates classes for schemas in definitions")]
        public void GeneratesClassesForSchemasInDefinitions()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = TestUtil.CreateSchemaFromTestDataFile("Definitions");

            generator.Generate(schema);

            const string ExpectedRootClass =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""rootProp"", IsRequired = false, EmitDefaultValue = false)]
        public bool RootProp { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + RootProp.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (RootProp != other.RootProp)
            {
                return false;
            }

            return true;
        }
    }
}";

            const string ExpectedDefinedClass1 =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class Def1 : IEquatable<Def1>
    {
        [DataMember(Name = ""prop1"", IsRequired = false, EmitDefaultValue = false)]
        public string Prop1 { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Def1);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Prop1 != null)
                {
                    result = (result * 31) + Prop1.GetHashCode();
                }
            }

            return result;
        }

        public bool Equals(Def1 other)
        {
            if (other == null)
            {
                return false;
            }

            if (Prop1 != other.Prop1)
            {
                return false;
            }

            return true;
        }
    }
}";

            const string ExpectedDefinedClass2 =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class Def2 : IEquatable<Def2>
    {
        [DataMember(Name = ""prop2"", IsRequired = false, EmitDefaultValue = false)]
        public int Prop2 { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Def2);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + Prop2.GetHashCode();
            }

            return result;
        }

        public bool Equals(Def2 other)
        {
            if (other == null)
            {
                return false;
            }

            if (Prop2 != other.Prop2)
            {
                return false;
            }

            return true;
        }
    }
}";
            string def1Path = TestFileSystem.MakeOutputFilePath("Def1");
            string def2Path = TestFileSystem.MakeOutputFilePath("Def2");

            var expectedOutputFiles = new List<string>
            {
                PrimaryOutputFilePath,
                def1Path,
                def2Path
            };

            _testFileSystem.Files.Count.Should().Be(expectedOutputFiles.Count);
            _testFileSystem.Files.Should().OnlyContain(path => expectedOutputFiles.Contains(path));

            _testFileSystem[PrimaryOutputFilePath].Should().Be(ExpectedRootClass);
            _testFileSystem[def1Path].Should().Be(ExpectedDefinedClass1);
            _testFileSystem[def2Path].Should().Be(ExpectedDefinedClass2);
        }

        [Fact(DisplayName = "DataModelGenerator generates date-time-valued properties")]
        public void GeneratesDateTimeValuedProperties()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
  ""startTime"": {
    ""type"": ""string"",
    ""format"": ""date-time""
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""startTime"", IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartTime { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + StartTime.GetHashCode();
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (StartTime != other.StartTime)
            {
                return false;
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates uri-valued properties")]
        public void GeneratesUriValuedProperties()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
  ""targetFile"": {
    ""type"": ""string"",
    ""format"": ""uri""
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""targetFile"", IsRequired = false, EmitDefaultValue = false)]
        public Uri TargetFile { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (TargetFile != null)
                {
                    result = (result * 31) + TargetFile.GetHashCode();
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (TargetFile != other.TargetFile)
            {
                return false;
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates integer property from dictionary reference")]
        public void GeneratesIntegerPropertyFromDictionaryReference()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""intDefProp"": {
      ""$ref"": ""#/definitions/d""
    },
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""integer""
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""intDefProp"", IsRequired = false, EmitDefaultValue = false)]
        public int IntDefProp { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of primitive types by $ref")]
        public void GeneratesArrayOfPrimitiveTypeByReference()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfIntByRef"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/d""
      }
    }
  },
  ""definitions"": {
    ""d"": {
      ""type"": ""integer"",
      }
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""arrayOfIntByRef"", IsRequired = false, EmitDefaultValue = false)]
        public IList<int> ArrayOfIntByRef { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (ArrayOfIntByRef != null)
                {
                    foreach (var value_0 in ArrayOfIntByRef)
                    {
                        result = result * 31;
                        result = (result * 31) + value_0.GetHashCode();
                    }
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(ArrayOfIntByRef, other.ArrayOfIntByRef))
            {
                if (ArrayOfIntByRef == null || other.ArrayOfIntByRef == null)
                {
                    return false;
                }

                if (ArrayOfIntByRef.Count != other.ArrayOfIntByRef.Count)
                {
                    return false;
                }

                for (int value_0 = 0; value_0 < ArrayOfIntByRef.Count; ++value_0)
                {
                    if (ArrayOfIntByRef[value_0] != other.ArrayOfIntByRef[value_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(Expected, actual, nameof(GeneratesArrayOfPrimitiveTypeByReference));

            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of primitive type")]
        public void GeneratesArrayOfArraysOfPrimitiveType()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfArrayOfInt"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/itemType""
      }
    }
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""integer""
      }
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""arrayOfArrayOfInt"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<int>> ArrayOfArrayOfInt { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (ArrayOfArrayOfInt != null)
                {
                    foreach (var value_0 in ArrayOfArrayOfInt)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                result = (result * 31) + value_1.GetHashCode();
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(ArrayOfArrayOfInt, other.ArrayOfArrayOfInt))
            {
                if (ArrayOfArrayOfInt == null || other.ArrayOfArrayOfInt == null)
                {
                    return false;
                }

                if (ArrayOfArrayOfInt.Count != other.ArrayOfArrayOfInt.Count)
                {
                    return false;
                }

                for (int value_0 = 0; value_0 < ArrayOfArrayOfInt.Count; ++value_0)
                {
                    if (!Object.ReferenceEquals(ArrayOfArrayOfInt[value_0], other.ArrayOfArrayOfInt[value_0]))
                    {
                        if (ArrayOfArrayOfInt[value_0] == null || other.ArrayOfArrayOfInt[value_0] == null)
                        {
                            return false;
                        }

                        if (ArrayOfArrayOfInt[value_0].Count != other.ArrayOfArrayOfInt[value_0].Count)
                        {
                            return false;
                        }

                        for (int value_1 = 0; value_1 < ArrayOfArrayOfInt[value_0].Count; ++value_1)
                        {
                            if (ArrayOfArrayOfInt[value_0][value_1] != other.ArrayOfArrayOfInt[value_0][value_1])
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(Expected, actual, nameof(GeneratesArrayOfArraysOfPrimitiveType));

            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of object type")]
        public void GeneratesArrayOfArraysOfObjectType()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfArrayOfObject"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/itemType""
      }
    }
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object""
      }
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""arrayOfArrayOfObject"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<object>> ArrayOfArrayOfObject { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (ArrayOfArrayOfObject != null)
                {
                    foreach (var value_0 in ArrayOfArrayOfObject)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                if (value_1 != null)
                                {
                                    result = (result * 31) + value_1.GetHashCode();
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(ArrayOfArrayOfObject, other.ArrayOfArrayOfObject))
            {
                if (ArrayOfArrayOfObject == null || other.ArrayOfArrayOfObject == null)
                {
                    return false;
                }

                if (ArrayOfArrayOfObject.Count != other.ArrayOfArrayOfObject.Count)
                {
                    return false;
                }

                for (int value_0 = 0; value_0 < ArrayOfArrayOfObject.Count; ++value_0)
                {
                    if (!Object.ReferenceEquals(ArrayOfArrayOfObject[value_0], other.ArrayOfArrayOfObject[value_0]))
                    {
                        if (ArrayOfArrayOfObject[value_0] == null || other.ArrayOfArrayOfObject[value_0] == null)
                        {
                            return false;
                        }

                        if (ArrayOfArrayOfObject[value_0].Count != other.ArrayOfArrayOfObject[value_0].Count)
                        {
                            return false;
                        }

                        for (int value_1 = 0; value_1 < ArrayOfArrayOfObject[value_0].Count; ++value_1)
                        {
                            if (!Object.Equals(ArrayOfArrayOfObject[value_0][value_1], other.ArrayOfArrayOfObject[value_0][value_1]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(Expected, actual, nameof(GeneratesArrayOfArraysOfObjectType));

            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates array of arrays of class type")]
        public void GeneratesArrayOfArraysOfClassType()
        {
            _settings.GenerateOverrides = true;
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
@"{
  ""type"": ""object"",
  ""properties"": {
    ""arrayOfArrayOfD"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/itemType""
      }
    }
  },
  ""definitions"": {
    ""itemType"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/definitions/d""
      }
    },
    ""d"": {
      ""type"": ""object"",
      }
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""arrayOfArrayOfD"", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<D>> ArrayOfArrayOfD { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as C);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (ArrayOfArrayOfD != null)
                {
                    foreach (var value_0 in ArrayOfArrayOfD)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            foreach (var value_1 in value_0)
                            {
                                result = result * 31;
                                if (value_1 != null)
                                {
                                    result = (result * 31) + value_1.GetHashCode();
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.ReferenceEquals(ArrayOfArrayOfD, other.ArrayOfArrayOfD))
            {
                if (ArrayOfArrayOfD == null || other.ArrayOfArrayOfD == null)
                {
                    return false;
                }

                if (ArrayOfArrayOfD.Count != other.ArrayOfArrayOfD.Count)
                {
                    return false;
                }

                for (int value_0 = 0; value_0 < ArrayOfArrayOfD.Count; ++value_0)
                {
                    if (!Object.ReferenceEquals(ArrayOfArrayOfD[value_0], other.ArrayOfArrayOfD[value_0]))
                    {
                        if (ArrayOfArrayOfD[value_0] == null || other.ArrayOfArrayOfD[value_0] == null)
                        {
                            return false;
                        }

                        if (ArrayOfArrayOfD[value_0].Count != other.ArrayOfArrayOfD[value_0].Count)
                        {
                            return false;
                        }

                        for (int value_1 = 0; value_1 < ArrayOfArrayOfD[value_0].Count; ++value_1)
                        {
                            if (!Object.Equals(ArrayOfArrayOfD[value_0][value_1], other.ArrayOfArrayOfD[value_0][value_1]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}";
            string actual = generator.Generate(schema);

            TestUtil.WriteTestResultFiles(Expected, actual, nameof(GeneratesArrayOfArraysOfClassType));

            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates string for inline enum of string")]
        public void GeneratesStringForInlineEnumOfString()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
    @"{
  ""type"": ""object"",
  ""properties"": {
    ""version"": {
      ""enum"": [
        ""v1.0"",
        ""v2.0""
      ]
    }
  }
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""version"", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }

        [Fact(DisplayName = "DataModelGenerator generates attributes for required and optional properties")]
        public void GeneratesAttributesForRequiredAndOptionalProperties()
        {
            var generator = new DataModelGenerator(_settings, _testFileSystem.FileSystem);

            JsonSchema schema = SchemaReader.ReadSchema(
    @"{
  ""type"": ""object"",
  ""properties"": {
    ""requiredProp1"": {
      ""type"": ""string""
    },
    ""optionalProp"": {
      ""type"": ""string""
    },
    ""requiredProp2"": {
      ""type"": ""string""
    }
  },
  ""required"": [ ""requiredProp1"", ""requiredProp2"" ]
}");

            const string Expected =
@"using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode(""Microsoft.Json.Schema.ToDotNet"", ""0.6.0.0"")]
    public sealed class C : IEquatable<C>
    {
        [DataMember(Name = ""requiredProp1"", IsRequired = true)]
        public string RequiredProp1 { get; set; }
        [DataMember(Name = ""optionalProp"", IsRequired = false, EmitDefaultValue = false)]
        public string OptionalProp { get; set; }
        [DataMember(Name = ""requiredProp2"", IsRequired = true)]
        public string RequiredProp2 { get; set; }
    }
}";
            string actual = generator.Generate(schema);
            actual.Should().Be(Expected);
        }
    }
}
