// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("JSON Schema To .NET Library")]
[assembly: AssemblyDescription("Classes for generating .NET classes from a JSON Schema")]

[assembly: InternalsVisibleTo("Microsoft.Json.Schema.UnitTests")]

// This allows Moq to mock internal interfaces of assemblies that are not strong named.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
