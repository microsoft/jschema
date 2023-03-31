# Microsoft Json Schema Packages

## **2.4.0** UNRELEASED
* BREAKING: Merge `HintKind`: `PropertyNameHint` and `PropertyModifiersHint` into one `PropertyHint` and change the setting `dotNetPropertyName` to `name`. [#171](https://github.com/microsoft/jschema/pull/171)
* FEATURE: Add a new setting `typeName` in `PropertyHint` to generate the specified .NET type, instead of deriving from the JSON schema. Supported values: `int`, `long`, `BigInteger`, `double`, `float`, `decimal`, `DateTime`, `Uri`, `Guid`, `bool`, `string`. [#171](https://github.com/microsoft/jschema/pull/171)

## **2.3.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/2.3.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/2.3.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/2.3.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/2.3.0)
* FEATURE: Added support to validate JSON against string format attribute from JSON Schema. [#169](https://github.com/microsoft/jschema/pull/169)

## **2.2.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/2.2.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/2.2.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/2.2.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/2.2.0)
* BREAKING: .NET type to express Json integers now will be nullable if the property is not required and also without default. [#167](https://github.com/microsoft/jschema/pull/167)
* FEATURE: Add new option for specifying .NET type to express Json numbers: `--generate-json-number-as = double | float | decimal` with a default of `double`. [#166](https://github.com/microsoft/jschema/pull/166)

## **2.1.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/2.1.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/2.1.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/2.1.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/2.1.0)
* FEATURE: Add support for JSON Schema type `uuid` generate as C# nullable `Guid?`. [#164](https://github.com/microsoft/jschema/pull/164)

## **2.0.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/2.0.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/2.0.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/2.0.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/2.0.0)
* Loosen Newtonsoft.JSON minimum version requirement from v13.0.1 to v9.0.1 [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/9.0.1). [#157](https://github.com/microsoft/jschema/pull/157)
* FEATURE: Add support for JSON Schema type `uuid`. [#132](https://github.com/microsoft/jschema/pull/132)
* FEATURE: Add new option for specifying .NET type to express Json integers: `--generate-json-integer-as = int | long | biginteger | auto` with a default of `int`. [#158](https://github.com/microsoft/jschema/pull/158)

## **1.1.5** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.5) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.4)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.4)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.5)
* Updating [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/13.0.1) from v9.0.1 to v13.0.1 in response to [Advisory: Improper Handling of Exceptional Conditions in Newtonsoft.Json](https://github.com/advisories/GHSA-5crp-9r3c-p9vr). [#155](https://github.com/microsoft/jschema/pull/155)

## **1.1.4** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.4) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.4)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.4)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.4)

* Update validator to use latest SARIF SDK (2.4.14).
* Fixing wrong array logic.
* Enable symbols.

## **1.1.3** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.3) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.3)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.3)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.3)

* Improve error messages: Remove unnecessary words; fix some typos; unify on single quotes.
* Update validator to use latest SARIF SDK (2.3.3).
* Add end-to-end test of validation.
* Fix bug in validation of arrays with item schemas.
* Updating client tool to support netcoreapp3.1.

## **1.1.2** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.2) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.2)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.2)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.2)

* [#126](https://github.com/microsoft/jschema/issues/126): Implement value equality on `JsonPointer`.

## **1.1.1** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.1) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.1)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.1)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.1)

* For the sake of compatibility with consumers who both (1) use JSchema (which depends on Newtonsoft.Json 9.0.1) and (2) use other packages which depend on Newtonsoft.Json 11 or later, we remove the internal `ExtensionCapturingTraceWriter` class.
There is no change in behavior. The functionality of the removed class is now implemented in a different way.

    This change is related to a long-standing problem in Newtonsoft.Json (see https://github.com/JamesNK/Newtonsoft.Json/issues/1616). Version 11 fixed that problem by removing the type `Newtonsoft.Json.TraceLevel` entirely
(that is, now, both the net461 and netstandard2.0 libraries use `System.Diagnostics.TraceLevel`). But JSchema, relying as it does on 9.0.1, _requires_ `Netwonsoft.Json.TraceLevel` to implement an `ITraceWriter`.
In an application with the dependencies described above, the higher version of Newtonsoft.Json will be loaded, the type `Newtonsoft.Json.TraceLevel` will not be available, and so the type initializer for `ExtensionCapturingTraceWriter` will fail.

## **1.1.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.1.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.1.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.1.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.1.0)

* [#118](https://github.com/microsoft/jschema/issues/118): Code gen: Allow generated Init methods to be protected.

## **1.0.1** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.0.1) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.0.1)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.0.1)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.0.1)

* [#108](https://github.com/microsoft/jschema/issues/108): Bug fix: The code generator crashed on a schema with `"type": "array"` but no `"items"` property.
According to JSON Schema, that is equivalent to `"items": { }`, meaning anything is allowed, meaning this construct should generate an array whose elements are `System.Object`.

## **1.0.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/1.0.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/1.0.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/1.0.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/1.0.0)

* Update SARIF dependency to v2.1.0.

## **0.63.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.63.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.63.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.63.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.63.0)

* Update SARIF dependency to private build which is effectively 2.0.0-csd.2.beta.2019-02-20 (the core SARIF eballots).

## **0.62.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.62.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.62.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.62.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.62.0)

* Update SARIF dependency to 2.0.0-csd.2.beta.2019-01-24.1

## **0.61.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.61.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.61.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.61.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.61.0)

* [#84](https://github.com/microsoft/jschema/issues/84): Bug fix: Generated default values for enumerated types now work properly.

## **0.60.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.60.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.60.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.60.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.60.0)

* [#68](https://github.com/microsoft/jschema/issues/68), [#69](https://github.com/microsoft/jschema/issues/69): Fix up NuGet package attributes related to licensing.
* [#77](https://github.com/microsoft/jschema/issues/77): Bug fix: Don't emit DefaultValue attributes for non-primitive types.
* [#79](https://github.com/microsoft/jschema/issues/79): Code gen: Add limited support for `oneOf` to allow an array-valued property to have a `null` value.

## **0.59.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.59.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.59.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.59.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.59.0)
* Add missing runtime dependencies to Validation and ToDotNet packages.
* Drop JSON.NET dependency to 9.0.1.
* Provide handling to populate default property values during deserialization and on object creation.
* In ToDotNet, emit initializers in the default constructor, and emit a `JsonProperty` attribute, for all properties whose default values differ from the .NET defaults. This ensures that these properties are properly initialized whether the object is default-constructed or deserialized from JSON.
* Update the `<license>` elements in the .nuspec files to conform to recent NuGet improvements in this area.

## **0.58.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.58.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.58.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.58.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.58.0)
* Package validation and .NET object model generation utilities in Validation and ToDotNet packages.

## **0.57.0** [Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer/0.57.0) | [Schema](https://www.nuget.org/packages/Microsoft.Json.Schema/0.57.0)| [Schema.ToDotNet](https://www.nuget.org/packages/Microsoft.Json.Schema.ToDotNet/0.57.0)| [Schema.Validation](https://www.nuget.org/packages/Microsoft.Json.Schema.Validation/0.57.0)
* The SARIF log file produced by the validation tool no longer uses the obsolete property result.ruleMessageId.
