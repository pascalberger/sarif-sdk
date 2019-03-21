// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Run for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class RunEqualityComparer : IEqualityComparer<Run>
    {
        internal static readonly RunEqualityComparer Instance = new RunEqualityComparer();

        public bool Equals(Run left, Run right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Tool.ValueComparer.Equals(left.Tool, right.Tool))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Invocations, right.Invocations))
            {
                if (left.Invocations == null || right.Invocations == null)
                {
                    return false;
                }

                if (left.Invocations.Count != right.Invocations.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Invocations.Count; ++index_0)
                {
                    if (!Invocation.ValueComparer.Equals(left.Invocations[index_0], right.Invocations[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!Conversion.ValueComparer.Equals(left.Conversion, right.Conversion))
            {
                return false;
            }

            if (left.Language != right.Language)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.VersionControlProvenance, right.VersionControlProvenance))
            {
                if (left.VersionControlProvenance == null || right.VersionControlProvenance == null)
                {
                    return false;
                }

                if (left.VersionControlProvenance.Count != right.VersionControlProvenance.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.VersionControlProvenance.Count; ++index_1)
                {
                    if (!VersionControlDetails.ValueComparer.Equals(left.VersionControlProvenance[index_1], right.VersionControlProvenance[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.OriginalUriBaseIds, right.OriginalUriBaseIds))
            {
                if (left.OriginalUriBaseIds == null || right.OriginalUriBaseIds == null || left.OriginalUriBaseIds.Count != right.OriginalUriBaseIds.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.OriginalUriBaseIds)
                {
                    ArtifactLocation value_1;
                    if (!right.OriginalUriBaseIds.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!ArtifactLocation.ValueComparer.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Artifacts, right.Artifacts))
            {
                if (left.Artifacts == null || right.Artifacts == null)
                {
                    return false;
                }

                if (left.Artifacts.Count != right.Artifacts.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.Artifacts.Count; ++index_2)
                {
                    if (!Artifact.ValueComparer.Equals(left.Artifacts[index_2], right.Artifacts[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null || right.LogicalLocations == null)
                {
                    return false;
                }

                if (left.LogicalLocations.Count != right.LogicalLocations.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < left.LogicalLocations.Count; ++index_3)
                {
                    if (!LogicalLocation.ValueComparer.Equals(left.LogicalLocations[index_3], right.LogicalLocations[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Graphs, right.Graphs))
            {
                if (left.Graphs == null || right.Graphs == null)
                {
                    return false;
                }

                if (left.Graphs.Count != right.Graphs.Count)
                {
                    return false;
                }

                for (int index_4 = 0; index_4 < left.Graphs.Count; ++index_4)
                {
                    if (!Graph.ValueComparer.Equals(left.Graphs[index_4], right.Graphs[index_4]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Results, right.Results))
            {
                if (left.Results == null || right.Results == null)
                {
                    return false;
                }

                if (left.Results.Count != right.Results.Count)
                {
                    return false;
                }

                for (int index_5 = 0; index_5 < left.Results.Count; ++index_5)
                {
                    if (!Result.ValueComparer.Equals(left.Results[index_5], right.Results[index_5]))
                    {
                        return false;
                    }
                }
            }

            if (!RunAutomationDetails.ValueComparer.Equals(left.Id, right.Id))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.AggregateIds, right.AggregateIds))
            {
                if (left.AggregateIds == null || right.AggregateIds == null)
                {
                    return false;
                }

                if (left.AggregateIds.Count != right.AggregateIds.Count)
                {
                    return false;
                }

                for (int index_6 = 0; index_6 < left.AggregateIds.Count; ++index_6)
                {
                    if (!RunAutomationDetails.ValueComparer.Equals(left.AggregateIds[index_6], right.AggregateIds[index_6]))
                    {
                        return false;
                    }
                }
            }

            if (left.BaselineInstanceGuid != right.BaselineInstanceGuid)
            {
                return false;
            }

            if (left.MarkdownMessageMimeType != right.MarkdownMessageMimeType)
            {
                return false;
            }

            if (left.RedactionToken != right.RedactionToken)
            {
                return false;
            }

            if (left.DefaultFileEncoding != right.DefaultFileEncoding)
            {
                return false;
            }

            if (left.DefaultSourceLanguage != right.DefaultSourceLanguage)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.NewlineSequences, right.NewlineSequences))
            {
                if (left.NewlineSequences == null || right.NewlineSequences == null)
                {
                    return false;
                }

                if (left.NewlineSequences.Count != right.NewlineSequences.Count)
                {
                    return false;
                }

                for (int index_7 = 0; index_7 < left.NewlineSequences.Count; ++index_7)
                {
                    if (left.NewlineSequences[index_7] != right.NewlineSequences[index_7])
                    {
                        return false;
                    }
                }
            }

            if (left.ColumnKind != right.ColumnKind)
            {
                return false;
            }

            if (!ExternalPropertyFileReferences.ValueComparer.Equals(left.ExternalPropertyFileReferences, right.ExternalPropertyFileReferences))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.ThreadFlowLocations, right.ThreadFlowLocations))
            {
                if (left.ThreadFlowLocations == null || right.ThreadFlowLocations == null)
                {
                    return false;
                }

                if (left.ThreadFlowLocations.Count != right.ThreadFlowLocations.Count)
                {
                    return false;
                }

                for (int index_8 = 0; index_8 < left.ThreadFlowLocations.Count; ++index_8)
                {
                    if (!ThreadFlowLocation.ValueComparer.Equals(left.ThreadFlowLocations[index_8], right.ThreadFlowLocations[index_8]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Taxonomies, right.Taxonomies))
            {
                if (left.Taxonomies == null || right.Taxonomies == null)
                {
                    return false;
                }

                if (left.Taxonomies.Count != right.Taxonomies.Count)
                {
                    return false;
                }

                for (int index_9 = 0; index_9 < left.Taxonomies.Count; ++index_9)
                {
                    if (!ReportingDescriptor.ValueComparer.Equals(left.Taxonomies[index_9], right.Taxonomies[index_9]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Addresses, right.Addresses))
            {
                if (left.Addresses == null || right.Addresses == null)
                {
                    return false;
                }

                if (left.Addresses.Count != right.Addresses.Count)
                {
                    return false;
                }

                for (int index_10 = 0; index_10 < left.Addresses.Count; ++index_10)
                {
                    if (!Address.ValueComparer.Equals(left.Addresses[index_10], right.Addresses[index_10]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Translations, right.Translations))
            {
                if (left.Translations == null || right.Translations == null)
                {
                    return false;
                }

                if (left.Translations.Count != right.Translations.Count)
                {
                    return false;
                }

                for (int index_11 = 0; index_11 < left.Translations.Count; ++index_11)
                {
                    if (!Translation.ValueComparer.Equals(left.Translations[index_11], right.Translations[index_11]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.Properties)
                {
                    SerializedPropertyInfo value_3;
                    if (!right.Properties.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!object.Equals(value_2.Value, value_3))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(Run obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Tool != null)
                {
                    result = (result * 31) + obj.Tool.ValueGetHashCode();
                }

                if (obj.Invocations != null)
                {
                    foreach (var value_4 in obj.Invocations)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.Language != null)
                {
                    result = (result * 31) + obj.Language.GetHashCode();
                }

                if (obj.VersionControlProvenance != null)
                {
                    foreach (var value_5 in obj.VersionControlProvenance)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.OriginalUriBaseIds != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in obj.OriginalUriBaseIds)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Artifacts != null)
                {
                    foreach (var value_7 in obj.Artifacts)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.LogicalLocations != null)
                {
                    foreach (var value_8 in obj.LogicalLocations)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Graphs != null)
                {
                    foreach (var value_9 in obj.Graphs)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Results != null)
                {
                    foreach (var value_10 in obj.Results)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.ValueGetHashCode();
                }

                if (obj.AggregateIds != null)
                {
                    foreach (var value_11 in obj.AggregateIds)
                    {
                        result = result * 31;
                        if (value_11 != null)
                        {
                            result = (result * 31) + value_11.ValueGetHashCode();
                        }
                    }
                }

                if (obj.BaselineInstanceGuid != null)
                {
                    result = (result * 31) + obj.BaselineInstanceGuid.GetHashCode();
                }

                if (obj.MarkdownMessageMimeType != null)
                {
                    result = (result * 31) + obj.MarkdownMessageMimeType.GetHashCode();
                }

                if (obj.RedactionToken != null)
                {
                    result = (result * 31) + obj.RedactionToken.GetHashCode();
                }

                if (obj.DefaultFileEncoding != null)
                {
                    result = (result * 31) + obj.DefaultFileEncoding.GetHashCode();
                }

                if (obj.DefaultSourceLanguage != null)
                {
                    result = (result * 31) + obj.DefaultSourceLanguage.GetHashCode();
                }

                if (obj.NewlineSequences != null)
                {
                    foreach (var value_12 in obj.NewlineSequences)
                    {
                        result = result * 31;
                        if (value_12 != null)
                        {
                            result = (result * 31) + value_12.GetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.ColumnKind.GetHashCode();
                if (obj.ExternalPropertyFileReferences != null)
                {
                    result = (result * 31) + obj.ExternalPropertyFileReferences.ValueGetHashCode();
                }

                if (obj.ThreadFlowLocations != null)
                {
                    foreach (var value_13 in obj.ThreadFlowLocations)
                    {
                        result = result * 31;
                        if (value_13 != null)
                        {
                            result = (result * 31) + value_13.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Taxonomies != null)
                {
                    foreach (var value_14 in obj.Taxonomies)
                    {
                        result = result * 31;
                        if (value_14 != null)
                        {
                            result = (result * 31) + value_14.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Addresses != null)
                {
                    foreach (var value_15 in obj.Addresses)
                    {
                        result = result * 31;
                        if (value_15 != null)
                        {
                            result = (result * 31) + value_15.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Translations != null)
                {
                    foreach (var value_16 in obj.Translations)
                    {
                        result = result * 31;
                        if (value_16 != null)
                        {
                            result = (result * 31) + value_16.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_17 in obj.Properties)
                    {
                        xor_1 ^= value_17.Key.GetHashCode();
                        if (value_17.Value != null)
                        {
                            xor_1 ^= value_17.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}