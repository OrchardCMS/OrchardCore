using System.Collections.Immutable;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNode : AdminNode
    {
        public bool ShowAll { get; set; }
        public string IconClass { get; set; }
        public ImmutableArray<ContentTypeEntry> ContentTypes { get; set; } = ImmutableArray<ContentTypeEntry>.Empty;
    }

    public class ContentTypeEntry
    {
        public string ContentTypeId { get; set; }
        public string IconClass { get; set; }
    }
}
