using System;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Lists.ViewModels;

public class ListPartHeaderAdminViewModel
{
    public ContentItem ContainerContentItem { get; set; }

    public ContentTypeDefinition[] ContainedContentTypeDefinitions { get; set; } = Array.Empty<ContentTypeDefinition>();

    public bool EnableOrdering { get; set; }
}
