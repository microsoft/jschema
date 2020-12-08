# Microsoft/jschema

A set of .NET components for working with JSON Schema Draft 4

The JSchema repo consists of the following components, each of which is documented in its own README.md:

* `Microsoft.Json.Pointer`: an implementation of the JSON Pointer specification ([RFC 6901](https://tools.ietf.org/html/rfc6901)). Documentation: [src/Json.Pointer/README.md](src/Json.Pointer/README.md).

* `Microsoft.Json.Schema`: an almost but not quite complete implementation of [JSON Schema Draft 4](http://json-schema.org/specification-links.html#draft-4). Documentation: coming soon.

* `Microsoft.Json.Schema.Validation`: a library to validate a JSON instance document against a JSON schema document. Documentation: coming soon.

* `Microsoft.Json.Schema.Validation.Cli`: a command-line tool to validate a JSON instance document against a JSON schema document, built on the `Microsoft.Json.Schema.Validation` library.

* `Microsoft.Json.Schema.ToDotNet`: a library to generate .NET classes from a JSON schema. Documentation (incomplete): [src/Json.Schema.ToDotNet/README.md](src/Json.Schema.ToDotNet/README.md).

* `Microsoft.Json.Schema.ToDotNet.Cli`: a command-line tool to generate .NET classes from a JSON schema, built on the `Microsoft.Json.Schema.ToDotNet` library. Documentation: coming soon.

All facilities built from the JSchema repo, including the command line tools, are available for both the net461 and netcoreapp2.1 platforms.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/),
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
