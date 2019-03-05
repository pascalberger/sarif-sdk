// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Information about how a specific tool report was reconfigured at runtime.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ReportingConfigurationOverride : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ReportingConfigurationOverride> ValueComparer => ReportingConfigurationOverrideEqualityComparer.Instance;

        public bool ValueEquals(ReportingConfigurationOverride other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ReportingConfigurationOverride;
            }
        }

        /// <summary>
        /// Specifies how the report was configured during the scan.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public ReportingConfiguration Configuration { get; set; }

        /// <summary>
        /// A reference that can be used to locate the reporting descriptor associated with this reporting configuration override.
        /// </summary>
        [DataMember(Name = "notificationDescriptorReference", IsRequired = false, EmitDefaultValue = false)]
        public ReportingDescriptorReference NotificationDescriptorReference { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the reporting configuration.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingConfigurationOverride" /> class.
        /// </summary>
        public ReportingConfigurationOverride()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingConfigurationOverride" /> class from the supplied values.
        /// </summary>
        /// <param name="configuration">
        /// An initialization value for the <see cref="P:Configuration" /> property.
        /// </param>
        /// <param name="notificationDescriptorReference">
        /// An initialization value for the <see cref="P:NotificationDescriptorReference" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ReportingConfigurationOverride(ReportingConfiguration configuration, ReportingDescriptorReference notificationDescriptorReference, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(configuration, notificationDescriptorReference, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingConfigurationOverride" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ReportingConfigurationOverride(ReportingConfigurationOverride other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Configuration, other.NotificationDescriptorReference, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ReportingConfigurationOverride DeepClone()
        {
            return (ReportingConfigurationOverride)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ReportingConfigurationOverride(this);
        }

        private void Init(ReportingConfiguration configuration, ReportingDescriptorReference notificationDescriptorReference, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (configuration != null)
            {
                Configuration = new ReportingConfiguration(configuration);
            }

            if (notificationDescriptorReference != null)
            {
                NotificationDescriptorReference = new ReportingDescriptorReference(notificationDescriptorReference);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}