using System;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Lists.ViewModels;

public class ListPartNavigationViewModel
{
    public string ContainerId { get; set; }

    public ContentTypeDefinition ContainerContentTypeDefinition { get; set; }

    public bool EnableOrdering { get; set; }

    public ContentTypeDefinition[] ContainedContentTypeDefinitions { get; set; } = Array.Empty<ContentTypeDefinition>();
}
