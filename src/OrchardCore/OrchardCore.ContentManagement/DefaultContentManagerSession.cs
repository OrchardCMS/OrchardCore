using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    public class DefaultContentManagerSession : IContentManagerSession
    {
        private readonly Dictionary<long, ContentItem> _itemByVersionId = new();
        private readonly Dictionary<string, ContentItem> _publishedItemsById = new();

        private bool _hasItems;

        public void Store(ContentItem item)
        {
            _hasItems = true;

            // Don't fail to re-add an item if it is the same instance.
            if (!_itemByVersionId.TryGetValue(item.Id, out var existing) || existing != item)
            {
                _itemByVersionId.Add(item.Id, item);
            }

            // Is it the Published version?
            if (item.Published)
            {
                _publishedItemsById[item.ContentItemId] = item;
            }
        }

        public bool RecallVersionId(long id, out ContentItem item)
        {
            if (!_hasItems)
            {
                item = null;
                return false;
            }

            return _itemByVersionId.TryGetValue(id, out item);
        }

        public bool RecallPublishedItemId(string id, out ContentItem item)
        {
            if (!_hasItems)
            {
                item = null;
                return false;
            }

            return _publishedItemsById.TryGetValue(id, out item);
        }

        public void Clear()
        {
            _itemByVersionId.Clear();
            _publishedItemsById.Clear();
            _hasItems = false;
        }
    }
}
