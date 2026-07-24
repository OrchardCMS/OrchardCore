using OrchardCore.ContentManagement;

namespace OrchardCore.Lists.ViewModels;

public class ListPartSummaryAdminViewModel
{
    public ContentItem ContentItem { get; set; }

    public string[] ContainedContentTypes { get; set; }
}
