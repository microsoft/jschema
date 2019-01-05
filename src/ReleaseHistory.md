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