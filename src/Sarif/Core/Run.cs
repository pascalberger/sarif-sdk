// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        private static Graph EmptyGraph = new Graph();
        private static Artifact EmptyFile = new Artifact();
        private static Invocation EmptyInvocation = new Invocation();
        private static LogicalLocation EmptyLogicalLocation = new LogicalLocation();

        private IDictionary<ArtifactLocation, int> _fileToIndexMap;

        public Uri ExpandUrisWithUriBaseId(string key, string currentValue = null)
        {
            ArtifactLocation artifactionLocation = this.OriginalUriBaseIds[key];

            if (artifactionLocation.UriBaseId == null)
            {
                return artifactionLocation.Uri;
            }
            throw new InvalidOperationException("Author this code along with tests for originalUriBaseIds that are nested");
        }

        public int GetFileIndex(
            ArtifactLocation artifactionLocation,
            bool addToFilesTableIfNotPresent = true,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Encoding encoding = null)
        {
            if (artifactionLocation == null) { throw new ArgumentNullException(nameof(artifactionLocation)); }

            if (this.Artifacts?.Count == 0)
            {
                if (!addToFilesTableIfNotPresent)
                {
                    return -1;
                }
            }

            if (_fileToIndexMap == null)
            {
                InitializeFileToIndexMap();
            }

            // Strictly speaking, some elements that may contribute to a files table 
            // key are case sensitive, e.g., everything but the schema and protocol of a
            // web URI. We don't have a proper comparer implementation that can handle 
            // all cases. For now, we cover the Windows happy path, which assumes that
            // most URIs in log files are file paths (which are case-insensitive)
            //
            // Tracking item for an improved comparer:
            // https://github.com/Microsoft/sarif-sdk/issues/973

            // When we perform a files table look-up, only the uri and uriBaseId
            // are relevant; these properties together comprise the unique identity
            // of the file object. The file index, of course, does not relate to the
            // file identity. We consciously exclude the properties bag as well.

            // We will normalize the input artifactionLocation.Uri to make URIs more consistent
            // throughout the emitted log.
            artifactionLocation.Uri = new Uri(UriHelper.MakeValidUri(artifactionLocation.Uri.OriginalString), UriKind.RelativeOrAbsolute);

            var filesTableKey = new ArtifactLocation
            {
                Uri = artifactionLocation.Uri,
                UriBaseId = artifactionLocation.UriBaseId
            };

            if (!_fileToIndexMap.TryGetValue(filesTableKey, out int fileIndex))
            {
                if (addToFilesTableIfNotPresent)
                {
                    this.Artifacts = this.Artifacts ?? new List<Artifact>();
                    fileIndex = this.Artifacts.Count;

                    string mimeType = Writers.MimeType.DetermineFromFileExtension(filesTableKey.Uri.ToString());

                    var artifact = Artifact.Create(
                        filesTableKey.Uri,
                        dataToInsert,
                        mimeType: mimeType,
                        encoding);

                    artifact.Location = artifactionLocation;

                    this.Artifacts.Add(artifact);

                    _fileToIndexMap[filesTableKey] = fileIndex;
                }
                else
                {
                    // We did not find the item. The call was not configured to add the entry.
                    // Return the default value that indicates the item isn't present.
                    fileIndex = -1;
                }
            }

            artifactionLocation.Index = fileIndex;
            return fileIndex;
        }

        private void InitializeFileToIndexMap()
        {
            _fileToIndexMap = new Dictionary<ArtifactLocation, int>(ArtifactLocation.ValueComparer);

            // First, we'll initialize our file object to index map
            // with any files that already exist in the table
            for (int i = 0; i < this.Artifacts?.Count; i++)
            {
                Artifact artifact = this.Artifacts[i];

                var artifactionLocation = new ArtifactLocation
                {
                    Uri = artifact.Location?.Uri,
                    UriBaseId = artifact.Location?.UriBaseId,
                };

                _fileToIndexMap[artifactionLocation] = i;
            }
        }

        public bool ShouldSerializeColumnKind()
        {
            // This serialization helper does two things. 
            // 
            // First, if ColumnKind has not been 
            // explicitly set, we will set it to the value that works for the Microsoft 
            // platform (which is not the specified SARIF default). This makes sure that
            // the value is set appropriate for code running on the Microsoft platform, 
            // even if the SARIF producer is not aware of this rather obscure value. 
            if (this.ColumnKind == ColumnKind.None)
            {
                this.ColumnKind = ColumnKind.Utf16CodeUnits;
            }

            // Second, we will always explicitly serialize this value. Otherwise, we can't easily
            // distinguish between earlier versions of the format for which this property was typically absent.
            return true;
        }

        public bool ShouldSerializeFiles() { return this.Artifacts.HasAtLeastOneNonNullValue(); }

        public bool ShouldSerializeGraphs() { return this.Graphs.HasAtLeastOneNonNullValue(); }

        public bool ShouldSerializeInvocations() { return this.Invocations.HasAtLeastOneNonDefaultValue(Invocation.ValueComparer); }

        public bool ShouldSerializeLogicalLocations() { return this.LogicalLocations.HasAtLeastOneNonNullValue(); }

        public bool ShouldSerializeNewlineSequences() { return this.NewlineSequences.HasAtLeastOneNonNullValue(); }
    }
}
