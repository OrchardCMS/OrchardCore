using System;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNode : AdminNode
    {
        public bool ShowAll { get; set; }
        public string IconClass { get; set; }
        public ContentTypeEntry[] ContentTypes { get; set; } = Array.Empty<ContentTypeEntry>();
    }

    public class ContentTypeEntry
    {
        public string ContentTypeId { get; set; }
        public string IconClass { get; set; }
    }
}
