# Microsoft.Json.Pointer

`Microsoft.Json.Pointer` contains classes for working with JSON Pointers. To use this package effectively, you need to be familiar with the JSON Pointer specification ([RFC 6901](https://tools.ietf.org/html/rfc6901)).

## Installation and usage

To use `Microsoft.Json.Pointer` in your project, add a NuGet reference to the [`Microsoft.Json.Pointer` NuGet package](https://www.nuget.org/packages/Microsoft.Json.Pointer/).

To use the `Microsoft.Json.Pointer` classes in a source file, add the `using` directive:
```
using Microsoft.Json.Pointer;
```

## Syntax support

The JSON Pointer specification describes the syntax of a JSON pointer in three usage scenarios:

- When it appears as a normal, "free-standing" string.
- When it appears in a JSON document.
- When it appears as the fragment portion of a URI.

`Microsoft.Json.Pointer` supports all three usage scenarios.

## Parsing a JSON Pointer string

The `JsonPointer` class parses a string into an array of "reference tokens" as described by the JSON Pointer specification.

#### Example 1: Normal syntax

This code parses a JSON Pointer string in the normal syntax:
```
using System;
using Microsoft.Json.Pointer;
// ...
var jPointer = new JsonPointer("/ab/cd");

foreach (string referenceToken in jPointer.ReferenceTokens)
{
    Console.WriteLine(referenceToken);
}
```

The code displays the following output:
```
ab
cd
```

#### Example 2: URI fragment syntax
This code parses a JSON pointer from the fragment portion of a URI.
```
using System;
using Microsoft.Json.Pointer;
// ...
var uri = new Uri("https://www.example.com/schemas/example-schema.json#/c%25d/ef%5E");

string fragment = uri.Fragment;
Console.WriteLine($"Fragment: {fragment}");
Console.WriteLine();

var jPointer = new JsonPointer(fragment, JsonPointerRepresentation.UriFragment);

foreach (string referenceToken in jPointer.ReferenceTokens)
{
    Console.WriteLine(referenceToken);
}
```

The code displays the following output:
```
Fragment: #/c%25d/ef%5E

c%d
ef^
```

## Evaluating a JSON pointer against a JSON document

`Microsoft.Json.Pointer` works with `Newtonsoft.Json` to retrieve the value specified by a JSON pointer, using the method
```
JToken JsonPointer.Evaluate(JToken token)
```

NOTE: The `Microsoft.Json.Pointer` NuGet package declares a dependency on `Newtonsoft.Json`, so you don't have to add `Newtonsoft.Json` to your project by hand.

#### Example 3: Pointer evaluation
Consider this JSON document, assumed to exist at the path `C:\json\example.json`:
```
{
    "outer": {
        "inner": [
            42,
            54
        ]
    }
}
```

Then this code evaluates the JSON pointer `"/outer/inner/1"` to the integer value `54`:
```
using System.IO;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;
// ...
string documentText = File.ReadAllText(@"C:\json\example.json");
JToken documentToken = JToken.Parse(documentText);

const string PointerString = "/outer/inner/1";
var jPointer = new JsonPointer(PointerString);

JToken arrayElementToken = jPointer.Evaluate(documentToken);
int value = (int)arrayElementToken;

Console.WriteLine($"JSON value at \"{PointerString}\" is {value}.");
```

The code displays the following output:
```
JSON value at "/outer/inner/1" is 54.
```

## Pointer string manipulation methods

`Microsoft.Json.Pointer` offers methods for manipulating JSON pointer strings, implemented as extension methods on `System.String`.

NOTE: These methods only work properly for JSON Pointer strings in the "normal" syntax.

### `AtProperty(string propertyName)`

Given a string in the normal JSON pointer format, referring to an object-valued property in a JSON document, `AtProperty` appends the name of a property in the sub-object.

#### Example 4: Referring to a nested property

This code appends a nested property name to a JSON pointer string:
```
using System;
using Microsoft.Json.Pointer;
// ...
string outerPointer = "/outer";
string innerPointer = outerPointer.AtProperty("inner");

Console.WriteLine($"innerPointer = \"{innerPointer}\"");
```

The code displays the following output:
```
innerPointer = "/outer/inner"
```

### `AtIndex(int index)`

Given a string in the normal JSON pointer format, referring to an array-valued property in a JSON document, `AtIndex` appends an integer index into the array.

#### Example 5: Referring to an array element

This code appends an array element index to a JSON pointer string:
```
using System;
using Microsoft.Json.Pointer;
// ...
string arrayPointer = "/outer/inner";
string elementPointer = arrayPointer.AtIndex(1);

Console.WriteLine($"elementPointer = \"{elementPointer}\"");
```

The code displays the following output:
```
elementPointer = "/outer/inner/1"
```

### `EscapeJsonPointer(string propertyName)`

Given a property name, `EscapeJsonPointer` escapes the characters `~` and `/` with `~0` and `~1`
as described in the JSON Pointer specification, so the resulting string can be used as
a reference token in a JSON pointer.

#### Example 6: Escaping a property name

This code escapes the necessary characters in a property name:
```
using System;
using Microsoft.Json.Pointer;
// ...
string propertyName = "a/~b";
string escapedPropertyName = propertyName.EscapeJsonPointer(propertyName);

Console.WriteLine($"escapedPropertyName = \"{escapedPropertyName}\"");
```

The code displays the following output:
```
escapedPropertyName = "a~1~0b"
```

NOTE: This method would have been better named `EscapeJsonPropertyName`.

### `UnescapeJsonPointer(string escapedPropertyName)`

Given an escaped property name, `UnescapeJsonPointer` replaces the escape sequences `~0` and `~1`
with `~` and `/` as described in the JSON pointer specification.

#### Example 7: Unescaping a property name

This code unescapes the escape sequences in an escaped property name:
```
using System;
using Microsoft.Json.Pointer;
// ...
string escapedPropertyName = "a~1~0b";
string propertyName = escapedPropertyName.UnescapeJsonPointer(escapedPropertyName);

Console.WriteLine($"propertyName = \"{propertyName}\"");
```

The code displays the following output:
```
propertyName = "a/~b"
```

NOTE: This method would have been better named `UnescapeJsonPropertyName`.