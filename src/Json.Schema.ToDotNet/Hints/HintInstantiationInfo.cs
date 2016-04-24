// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.ToDotNet.Hints
{
    /// <summary>
    /// Contains the information necessary to instantiate a <see cref="CodeGenHint"/>.
    /// </summary>
    /// <remarks>
    /// This class exists to avoid arbitrary code execution attacks by way of the
    /// HintDictionary. Rather than the code generation hints file specifying arbitrary
    /// classes to instantiate, it instead specifies one of a fixed number of supported
    /// hints, along with values that parameterize specific instances of those hints.
    /// The <see cref="HintDictionary"/> class deserializes the hints file and
    /// explicitly instantiates only those <see cref="CodeGenHint"/>-derived classes
    /// that it supports.
    /// </remarks>
    public class HintInstantiationInfo
    {
        /// <summary>
        /// Gets or sets a value specifying which kind of code generation hint
        /// to instantiate.
        /// </summary>
        public HintKind Kind { get; set;}

        /// <summary>
        /// Gets or sets a set of values used to instantiate the code generation hint.
        /// </summary>
        public JObject Arguments { get; set; }
    }
}
