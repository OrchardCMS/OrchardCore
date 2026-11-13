using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Lists.ViewModels;

public class ListPartNavigationAdminViewModel
{
    public ContentItem Container { get; set; }

    public ContentTypeDefinition ContainerContentTypeDefinition { get; set; }

    public bool EnableOrdering { get; set; }

    public ContentTypeDefinition[] ContainedContentTypeDefinitions { get; set; } = [];

    [BindNever]
    public ContentTypePartDefinition TypePartDefinition { get; set; }
}
