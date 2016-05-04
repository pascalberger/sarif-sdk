﻿
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class AnalysisContext : IAnalysisContext
    {
        public Exception TargetLoadException { get; set; }

        public bool IsValidAnalysisTarget { get; set; }

        public IAnalysisLogger Logger { get; set; }

        public IRule Rule { get; set; }

        public PropertyBag Policy { get; set; }

        public string MimeType { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public Uri TargetUri { get; set; }

        public void Dispose() { }
    }
}