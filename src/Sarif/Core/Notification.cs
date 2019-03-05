// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Notification
    {
        public string Id { get; set; }

        public string RuleId { get; set; }

        public int RuleIndex { get; set; }
    }
}
