// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileDataTests
    {
        [Fact]
        public void FileData_Create_NullUri()
        {
            Action action = () => { Artifact.Create(null, OptionallyEmittedData.None); };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FileData_ComputeHashes()
        {
            string filePath = Path.GetTempFileName();
            string artifactContents = Guid.NewGuid().ToString();
            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, artifactContents);
                Artifact artifact = Artifact.Create(uri, OptionallyEmittedData.Hashes);
                artifact.Location.Should().Be(null);
                HashData hashes = HashUtilities.ComputeHashes(filePath);
                artifact.MimeType.Should().Be(MimeType.Binary);
                artifact.Contents.Should().BeNull();
                artifact.Hashes.Count.Should().Be(3);

                foreach (string algorithm in artifact.Hashes.Keys)
                {
                    switch (algorithm)
                    {
                        case "md5": { artifact.Hashes[algorithm].Should().Be(hashes.MD5); break; }
                        case "sha-1": { artifact.Hashes[algorithm].Should().Be(hashes.Sha1); break; }
                        case "sha-256": { artifact.Hashes[algorithm].Should().Be(hashes.Sha256); break; }
                        default: { true.Should().BeFalse(); break; /* unexpected algorithm kind */ }
                    }
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Theory]
        // Unknown files are regarded as binary
        [InlineData(".unknown", OptionallyEmittedData.BinaryFiles, true)]
        [InlineData(".unknown", OptionallyEmittedData.TextFiles, false)]
        [InlineData(".exe", OptionallyEmittedData.BinaryFiles | OptionallyEmittedData.TextFiles, true)]
        [InlineData(".cs", OptionallyEmittedData.BinaryFiles | OptionallyEmittedData.TextFiles, true)]
        [InlineData(".jar", OptionallyEmittedData.BinaryFiles, true)]
        [InlineData(".jar", OptionallyEmittedData.TextFiles, false)]
        [InlineData(".cs", OptionallyEmittedData.BinaryFiles, false)]
        [InlineData(".cs", OptionallyEmittedData.TextFiles, true)]
        [InlineData(".h", ~OptionallyEmittedData.BinaryFiles, true)]
        [InlineData(".docx", ~OptionallyEmittedData.BinaryFiles, false)]
        [InlineData(".dll", ~OptionallyEmittedData.TextFiles, true)]
        [InlineData(".cpp", ~OptionallyEmittedData.TextFiles, false)]
        public void FileData_PersistBinaryAndTextartifactContents(
            string fileExtension,
            OptionallyEmittedData dataToInsert,
            bool shouldBePersisted)
        {
            string filePath = Path.GetTempFileName() + fileExtension;
            string artifactContents = Guid.NewGuid().ToString();
            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllText(filePath, artifactContents);
                Artifact artifact = Artifact.Create(uri, dataToInsert);
                artifact.Location.Should().BeNull();

                if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes))
                {
                    artifact.Hashes.Should().NotBeNull();
                }
                else
                {
                    artifact.Hashes.Should().BeNull();
                }

                string encodedartifactContents = Convert.ToBase64String(File.ReadAllBytes(filePath));

                if (shouldBePersisted)
                {
                    artifact.Contents.Binary.Should().Be(encodedartifactContents);
                    artifact.Contents.Text.Should().BeNull();
                }
                else
                {
                    artifact.Contents.Should().BeNull();
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void FileData_PersistTextartifactContentsBigEndianUnicode()
        {
            Encoding encoding = Encoding.BigEndianUnicode;
            string filePath = Path.GetTempFileName() + ".cs";
            string textValue = "अचम्भा";
            byte[] artifactContents = encoding.GetBytes(textValue);

            Uri uri = new Uri(filePath);

            try
            {
                File.WriteAllBytes(filePath, artifactContents);
                Artifact artifact = Artifact.Create(uri, OptionallyEmittedData.TextFiles, mimeType: null, encoding: encoding);
                artifact.Location.Should().Be(null);
                artifact.MimeType.Should().Be(MimeType.CSharp);
                artifact.Hashes.Should().BeNull();

                string encodedartifactContents = encoding.GetString(artifactContents);
                artifact.Contents.Text.Should().Be(encodedartifactContents);
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void FileData_FileDoesNotExist()
        {
            // If a file does not exist, and we request file contents
            // persistence, the logger will not raise an exception
            string filePath = Path.GetTempFileName();
            Uri uri = new Uri(filePath);
            Artifact artifact = Artifact.Create(uri, OptionallyEmittedData.TextFiles);
            artifact.Location.Should().Be(null);
            artifact.MimeType.Should().Be(MimeType.Binary);
            artifact.Hashes.Should().BeNull();
            artifact.Contents.Should().BeNull();
        }

        [Fact]
        public void FileData_FileIsLocked()
        {
            string filePath = Path.GetTempFileName();
            Uri uri = new Uri(filePath);

            try
            {
                // Place an exclusive read lock on file, so that Artifact cannot access its contents.
                // This raises an IOException, which is swallowed by Artifact.Create
                using (var exclusiveAccessReader = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    Artifact artifact = Artifact.Create(uri, OptionallyEmittedData.TextFiles);
                    artifact.Location.Should().Be(null);
                    artifact.MimeType.Should().Be(MimeType.Binary);
                    artifact.Hashes.Should().BeNull();
                    artifact.Contents.Should().BeNull();
                }
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        [Fact]
        public void FileData_TextFileIsNotAccessibleDueToSecurity()
        {
            RunUnauthorizedAccessTextForFile(isTextFile: true);
        }

        [Fact]
        public void FileData_BinaryFileIsNotAccessibleDueToSecurity()
        {
            RunUnauthorizedAccessTextForFile(isTextFile: false);
        }

        [Fact]
        public void FileData_SerializeSingleFileRole()
        {
            Artifact artifact = Artifact.Create(new Uri("file:///example.cs"), OptionallyEmittedData.None);
            artifact.Roles = ArtifactRoles.AnalysisTarget;

            string result = JsonConvert.SerializeObject(artifact);

            result.Should().Be("{\"roles\":[\"analysisTarget\"],\"mimeType\":\"text/x-csharp\"}");
        }

        [Fact]
        public void FileData_SerializeMultipleFileRoles()
        {
            Artifact artifact = Artifact.Create(new Uri("file:///example.cs"), OptionallyEmittedData.None);
            artifact.Roles = ArtifactRoles.ResponseFile | ArtifactRoles.ResultFile;

            string actual = JsonConvert.SerializeObject(artifact);

            actual.Should().Be("{\"roles\":[\"responseFile\",\"resultFile\"],\"mimeType\":\"text/x-csharp\"}");
        }

        [Fact]
        public void FileData_DeserializeSingleFileRole()
        {
            Artifact actual = JsonConvert.DeserializeObject("{\"roles\":[\"analysisTarget\"],\"mimeType\":\"text/x-csharp\"}", typeof(Artifact)) as Artifact;
            actual.Roles.Should().Be(ArtifactRoles.AnalysisTarget);
        }

        [Fact]
        public void FileData_DeserializeMultipleFileRoles()
        {
            Artifact actual = JsonConvert.DeserializeObject("{\"roles\":[\"responseFile\",\"resultFile\"],\"mimeType\":\"text/x-csharp\"}", typeof(Artifact)) as Artifact;
            actual.Roles.Should().Be(ArtifactRoles.ResponseFile | ArtifactRoles.ResultFile);
        }

        private static void RunUnauthorizedAccessTextForFile(bool isTextFile)
        {
            string extension = isTextFile ? ".cs" : ".dll";
            string filePath = Path.GetFullPath(Guid.NewGuid().ToString()) + extension;
            Uri uri = new Uri(filePath);

            IFileSystem fileSystem = SetUnauthorizedAccessExceptionMock();

            Artifact artifact = Artifact.Create(
                uri,
                OptionallyEmittedData.TextFiles,
                mimeType: null,
                encoding: null,
                fileSystem: fileSystem);

            // We pass none here as the occurrence of UnauthorizedAccessException 
            // should result in non-population of any file contents.
            Validate(artifact, OptionallyEmittedData.None);
        }

        private static IFileSystem SetUnauthorizedAccessExceptionMock()
        {
            Mock<IFileSystem> mock = GetDefaultFileSystemMock();
            mock.Setup(fs => fs.ReadAllText(It.IsAny<string>(), It.IsAny<Encoding>())).Returns((string s, Encoding encoding) => { throw new UnauthorizedAccessException(); });
            mock.Setup(fs => fs.ReadAllBytes(It.IsAny<string>())).Returns((string s) => { throw new UnauthorizedAccessException(); });
            return mock.Object;
        }

        private static Mock<IFileSystem> GetDefaultFileSystemMock()
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);
            mock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns((string s) => { return true; });
            return mock;
        }

        private static void Validate(Artifact artifact, OptionallyEmittedData dataToInsert)
        {
            if (dataToInsert.HasFlag(OptionallyEmittedData.TextFiles))
            {
                artifact.Contents.Should().NotBeNull();
            }
            else
            {
                artifact.Contents.Should().BeNull();
            }


        }
    }
}