// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Static Analysis Results Format (SARIF) Version 2.0.0-csd.2.beta-2019-01-24 JSON Schema: a standard format for the output of static analysis tools.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class SarifLog : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<SarifLog> ValueComparer => SarifLogEqualityComparer.Instance;

        public bool ValueEquals(SarifLog other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.SarifLog;
            }
        }

        /// <summary>
        /// The URI of the JSON schema corresponding to the version.
        /// </summary>
        [DataMember(Name = "$schema", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(UriConverter))]
        public Uri SchemaUri { get; set; }

        /// <summary>
        /// The SARIF format version of this log file.
        /// </summary>
        [DataMember(Name = "version", IsRequired = true)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.SarifVersionConverter))]
        public SarifVersion Version { get; set; }

        /// <summary>
        /// The set of runs contained in this log file.
        /// </summary>
        [DataMember(Name = "runs", IsRequired = true)]
        public IList<Run> Runs { get; set; }

        /// <summary>
        /// References to external property files that share data between runs.
        /// </summary>
        [DataMember(Name = "inlineExternalPropertyFiles", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> InlineExternalPropertyFiles { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the log file.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLog" /> class.
        /// </summary>
        public SarifLog()
        {
            // NOTYETAUTOGENERATED: we have special object initialization needs for this type
            // Tracked in jschema repi as https://github.com/Microsoft/jschema/issues/95
            Version = SarifVersion.Current;
            SchemaUri = new Uri(SarifUtilities.SarifSchemaUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLog" /> class from the supplied values.
        /// </summary>
        /// <param name="schemaUri">
        /// An initialization value for the <see cref="P:SchemaUri" /> property.
        /// </param>
        /// <param name="version">
        /// An initialization value for the <see cref="P:Version" /> property.
        /// </param>
        /// <param name="runs">
        /// An initialization value for the <see cref="P:Runs" /> property.
        /// </param>
        /// <param name="inlineExternalPropertyFiles">
        /// An initialization value for the <see cref="P:InlineExternalPropertyFiles" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public SarifLog(Uri schemaUri, SarifVersion version, IEnumerable<Run> runs, IEnumerable<ExternalPropertyFile> inlineExternalPropertyFiles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(schemaUri, version, runs, inlineExternalPropertyFiles, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLog" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public SarifLog(SarifLog other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.SchemaUri, other.Version, other.Runs, other.InlineExternalPropertyFiles, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public SarifLog DeepClone()
        {
            return (SarifLog)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new SarifLog(this);
        }

        private void Init(Uri schemaUri, SarifVersion version, IEnumerable<Run> runs, IEnumerable<ExternalPropertyFile> inlineExternalPropertyFiles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (schemaUri != null)
            {
                SchemaUri = new Uri(schemaUri.OriginalString, schemaUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            Version = version;
            if (runs != null)
            {
                var destination_0 = new List<Run>();
                foreach (var value_0 in runs)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Run(value_0));
                    }
                }

                Runs = destination_0;
            }

            if (inlineExternalPropertyFiles != null)
            {
                var destination_1 = new List<ExternalPropertyFile>();
                foreach (var value_1 in inlineExternalPropertyFiles)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new ExternalPropertyFile(value_1));
                    }
                }

                InlineExternalPropertyFiles = destination_1;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}