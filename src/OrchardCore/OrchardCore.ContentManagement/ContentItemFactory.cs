using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.ContentManagement;

public class ContentItemFactory : IContentItemFactory
{
    private readonly IContentManager _contentManager;
    private readonly IContentItemIdGenerator _contentItemIdGenerator;
    private readonly IMemoryCache _memoryCache;

    public ContentItemFactory(
        IContentManager contentManager,
        IContentItemIdGenerator contentItemIdGenerator,
        IMemoryCache memoryCache)
    {
        _contentManager = contentManager;
        _contentItemIdGenerator = contentItemIdGenerator;
        _memoryCache = memoryCache;
    }

    public async Task<ContentItem> CreateAsync(string contentTypeId, string ownerId = null)
    {
        if (String.IsNullOrEmpty(contentTypeId))
        {
            throw new ArgumentException($"{nameof(contentTypeId)} cannot be empty.");
        }

        var cacheKey = $"{nameof(ContentItemFactory)}:{contentTypeId.ToLowerInvariant()}";

        if (!_memoryCache.TryGetValue(cacheKey, out ContentItem item))
        {
            // At this point, we know there is no cached content item for the given contentTypeId.
            // Create a new instance and cache it for the next request.
            item = await _contentManager.NewAsync(contentTypeId);

            _memoryCache.Set(cacheKey, item);
        }

        // Create a new ContentItem instance to ensure we do not change the ContentItem instance that is stored in memory when we set the Owner value.
        var contentItem = new ContentItem();

        contentItem.Merge(item);
        contentItem.Owner = ownerId;
        contentItem.ContentItemId = _contentItemIdGenerator.GenerateUniqueId(contentItem);

        return contentItem;
    }
}
