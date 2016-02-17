// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.JSchema
{
    internal interface IFileSystem
    {
        void CreateDirectory(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
    }
}
