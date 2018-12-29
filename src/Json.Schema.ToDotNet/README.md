# Microsoft.Json.Schema.ToDotNet

`Microsoft.Json.Schema.ToDotNet` contains classes for generating a .NET object model from a JSON schema..

## Installation and usage

To use `Microsoft.Json.Schema.ToDotNet` in your project, add a NuGet reference to the [`Microsoft.Json.Schema.ToDotNet` NuGet package](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/).
This package declares a NuGet dependency on the [`Microsoft.Json.Schema` NuGet package](https://www.nuget.org/packages/Microsoft.Json.Schema/)

To use the `Microsoft.Json.Schema.ToDotNet` classes in a source file, add the `using` directive:
```
using Microsoft.Json.Schema.ToDotNet;
```

You can take advantage of the .NET code generation facility immediately by using the `Microsoft.Json.Schema.ToDotNet.Cli` command line tool,
available in the same NuGet package.

## Basic usage

The simplest way to use `Microsoft.Json.Schema.ToDotNet` programmatically is to:

1. Instantiate a `DataModelGeneratorSettings` object, specifying parameters such as the output directory for the code generation. The doc comments in `DataModelGeneratorSettings.cs` should suffice to explain the available settings.

2. Instantiate a `DataModelGenerator` object, passing the `DataModelGeneratorSettings` object to the constructor.

3. Instantiate a `Microsoft.Json.Schema.JsonSchema` object from the JSON schema file.

4. Invoke the `DataModelGenerator`'s `Generate` method, passing the schema object.
    The generated code appears in the directory specified by `DataModelGeneratorSettings.OutputDirectory`.

For an example, see the main program of the `Microsoft.Json.Schema.ToDotNet.Cli` command line tool, whose command line options correspond to the properties of the `DataModelGeneratorSettings` class.
See [`src\Json.Schema.ToDotNet.Cli\Program.cs`]().

## Auxiliary classes

### Comparers

### Visitors

## Code generation hints

### AttributeHint



### DictionaryHint

### EnumHint

### 

## Implementation notes

The code generation is implemented with .NET Compiler Platform (_aka_ "Roslyn").
It could also have been implemented by synthesizing and concatenating textual source code fragments.

The bulk of the code generation is accomplished with direct calls to the methods of Roslyn `SyntaxFactory` class.
However, certain code patterns (such as checking a parameter for `null`) occur repeatedly,
so the implementation includes an internal class `Microsoft.Json.Schema.ToDotNet.SyntaxHelper` with methods such as
`NullParameterCheck` to encapsulate these patterns.
If while maintaining this code you identify a pattern that could profitably be encapsulated in a `SyntaxHelper` method,
feel free to do so. Feel free even if the pattern occurs only once, if you feel the encapsulation will make
the code generation more readable.