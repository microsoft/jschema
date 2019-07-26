# Microsoft Json Schema Packages

## **0.57.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.57.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.57.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.57.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.57.0)
* The SARIF log file produced by the validation tool no longer uses the obsolete property result.ruleMessageId.

## **0.58.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.58.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.58.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.58.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.58.0)
* Package validation and .NET object model generation utilities in Validation and ToDotNet packages.

## **0.59.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.59.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.59.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.59.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.59.0)
* Add missing runtime dependencies to Validation and ToDotNet packages.
* Drop JSON.NET dependency to 9.0.1.
* Provide handling to populate default property values during deserialization and on object creation.
* In ToDotNet, emit initializers in the default constructor, and emit a `JsonProperty` attribute, for all properties whose default values differ from the .NET defaults. This ensures that these properties are properly initialized whether the object is default-constructed or deserialized from JSON.
* Update the `<license>` elements in the .nuspec files to conform to recent NuGet improvements in this area.

## **0.60.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.60.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.60.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.60.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.60.0)

* #68, #69: Fix up NuGet package attributes related to licensing.
* #77: Bug fix: Don't emit DefaultValue attributes for non-primitive types.
* #79: Code gen: Add limited support for `oneOf` to allow an array-valued property to have a `null` value.

## **0.61.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.61.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.61.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.61.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.61.0)

* #84: Bug fix: Generated default values for enumerated types now work properly.

## **0.62.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.62.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.62.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.62.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.62.0)

* Update SARIF dependency to 2.0.0-csd.2.beta.2019-01-24.1

## **0.63.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.63.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.63.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.63.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.63.0)

* Update SARIF dependency to private build which is effectively 2.0.0-csd.2.beta.2019-02-20 (the core SARIF eballots).

## **1.0.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.0.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.0.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.0.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.0.0)

* Update SARIF dependency to v2.1.0.

## **1.0.1** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.0.1) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.0.1)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.0.1)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.0.1)

* #108: Bug fix: The code generator crashed on a schema with `"type": "array"` but no `"items"` property.
According to JSON Schema, that is equivalent to `"items": { }`, meaning anything is allowed, meaning this construct should generate an array whose elements are `System.Object`.

## **1.1.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.0)

* #118: Code gen: Allow generated Init methods to be protected.

## **1.1.1** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.1) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.1)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.1)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.1)

* For the sake of compatibility with consumers who both (1) use JSchema (which depends on Newtonsoft.Json 9.0.1) and (2) use other packages which depend on Newtonsoft.Json 11 or later, we remove the internal `ExtensionCapturingTraceWriter` class.
There is no change in behavior. The functionality of the removed class is now implemented in a different way.

This change is related to a long-standing problem in Newtonsoft.Json (see https://github.com/JamesNK/Newtonsoft.Json/issues/1616). Version 11 fixed that problem by removing the type `Newtonsoft.Json.TraceLevel` entirely
(that is, now, both the net461 and netstandard2.0 libraries use `System.Diagnostics.TraceLevel`). But JSchema, relying as it does on 9.0.1, _requires_ `Netwonsoft.Json.TraceLevel` to implement an `ITraceWriter`.
In an application with the dependencies described above, the higher version of Newtonsoft.Json will be loaded, the type `Newtonsoft.Json.TraceLevel` will not be available, and so the type initializer for `ExtensionCapturingTraceWriter` will fail.