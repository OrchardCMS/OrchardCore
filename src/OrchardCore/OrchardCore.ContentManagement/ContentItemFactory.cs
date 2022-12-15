using System;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement;

public class ContentItemFactory : IContentItemFactory
{
    private readonly IContentManager _contentManager;
    private readonly IContentItemIdGenerator _contentItemIdGenerator;
    private readonly ContentItemFactorySession _contentItemFactorySession;

    public ContentItemFactory(
        IContentManager contentManager,
        IContentItemIdGenerator contentItemIdGenerator,
        ContentItemFactorySession contentItemFactorySession)
    {
        _contentManager = contentManager;
        _contentItemIdGenerator = contentItemIdGenerator;
        _contentItemFactorySession = contentItemFactorySession;
    }

    public async Task<ContentItem> CreateAsync(string contentTypeId, string ownerId = null)
    {
        if (String.IsNullOrEmpty(contentTypeId))
        {
            throw new ArgumentException($"{nameof(contentTypeId)} cannot be empty.");
        }

        var item = _contentItemFactorySession.Get(contentTypeId);

        if (item == null)
        {
            item = await _contentManager.NewAsync(contentTypeId);

            _contentItemFactorySession.Store(item);
        }

        var contentItem = new ContentItem();

        contentItem.Merge(item);
        contentItem.Owner = ownerId;
        contentItem.ContentItemId = _contentItemIdGenerator.GenerateUniqueId(contentItem);

        return contentItem;
    }
}
