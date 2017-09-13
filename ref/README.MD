Microsoft.Json.Schema.Sarif.dll is a renamed copy of Sarif.dll version 1.6.0,
built from the SARIF SDK (https://github.com/Microsoft/sarif-sdk).

It is checked in here to avoid a circular dependency: The Sarif.Sdk package
relies on Microsoft.Json.Schema. But the JsonSchemaValidator tool uses SARIF
as its output format, so the Microsoft.Json.Schema package depends on the
Sarif.Sdk package.

It would be possible to refactor the packages to avoid the circular dependency,
but for now, for expediency, we use the direct reference check-in approach.

