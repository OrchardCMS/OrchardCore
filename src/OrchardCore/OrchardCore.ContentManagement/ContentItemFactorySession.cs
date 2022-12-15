using System;
using System.Collections.Concurrent;

namespace OrchardCore.ContentManagement;

public class ContentItemFactorySession
{
    private readonly ConcurrentDictionary<string, ContentItem> _contentItems = new(StringComparer.OrdinalIgnoreCase);

    public ContentItem Get(string contentItemId)
    {
        if (contentItemId != null && _contentItems.TryGetValue(contentItemId, out var item))
        {
            return item;
        }

        return null;
    }

    public void Store(ContentItem contentItem)
    {
        if (contentItem?.ContentType == null)
        {
            return;
        }

        _contentItems.TryAdd(contentItem.ContentType, contentItem);
    }
}
