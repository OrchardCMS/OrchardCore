using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Services;
public class ContentItemListFeature
{
    public IEnumerable<ContentItem> ContentItems { get; set; }

    public ContentItemListFeature(IEnumerable<ContentItem> contentItems)
    {
        ContentItems = contentItems;
    }
}
