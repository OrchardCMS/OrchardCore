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
    [Obsolete("use ContentTypeName and ContentTypeDisplayName, for compatiblity with pre 3.X OrchardCore sites")]
    public string ContentTypeId { get; set; }

    public string ContentTypeName { get; set; }

    public string ContentTypeDisplayName { get; set; }

    public string IconClass { get; set; }
}
