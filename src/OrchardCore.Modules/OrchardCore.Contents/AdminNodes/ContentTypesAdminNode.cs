using OrchardCore.AdminMenu.Models;

namespace OrchardCore.Contents.AdminNodes;

public class ContentTypesAdminNode : AdminNode
{
    public bool ShowAll { get; set; }

    public string IconClass { get; set; }

    public ContentTypeEntry[] ContentTypes { get; set; } = [];
}

public class ContentTypeEntry
{
    public string ContentTypeName { get; set; }

    public string ContentTypeDisplayName { get; set; }

    public string IconClass { get; set; }
}
