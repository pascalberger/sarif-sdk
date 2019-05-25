// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Map.Splitter
{
    public class JsonSplitterTests
    {
        private static ResourceExtractor Extractor = new ResourceExtractor(typeof(JsonSplitterTests));

        [Fact]
        public void JsonSplitter_Basic()
        {
            string sampleFilePath = @"Map.Sample.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"Map.Sample.json"));

            JsonSplitter splitter = new JsonSplitter(File.OpenRead(sampleFilePath));

            while (splitter.Next())
            {
                JsonToken token = splitter.Current;
                Trace.WriteLine($"@{token.ValueStartIndex} {token.TokenType} -> \"{splitter.CurrentValueString()}\"");
            }
        }
    }
}
