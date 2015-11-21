using System;
using System.Collections.Generic;

namespace Orchard.ContentManagement
{
    public class DefaultContentManagerSession : IContentManagerSession
    {
        private readonly IDictionary<int, ContentItem> _itemByVersionId = new Dictionary<int, ContentItem>();
        private readonly IDictionary<Tuple<int, int>, ContentItem> _itemByContentItemId = new Dictionary<Tuple<int, int>, ContentItem>();
        private readonly IDictionary<int, ContentItem> _publishedItemsById = new Dictionary<int, ContentItem>();

        public void Store(ContentItem item)
        {
            _itemByVersionId.Add(item.Id, item);
            _itemByContentItemId.Add(Tuple.Create(item.ContentItemId, item.Number), item);

            // is it the Published version ?
            if (item.Latest && item.Published)
            {
                _publishedItemsById[item.ContentItemId] = item;
            }
        }

        public bool RecallVersionId(int id, out ContentItem item)
        {
            return _itemByVersionId.TryGetValue(id, out item);
        }

        public bool RecallContentItemId(int contentItemId, int versionNumber, out ContentItem item)
        {
            return _itemByContentItemId.TryGetValue(Tuple.Create(contentItemId, versionNumber), out item);
        }

        public bool RecallPublishedItemId(int id, out ContentItem item)
        {
            return _publishedItemsById.TryGetValue(id, out item);
        }

        public void Clear()
        {
            _itemByVersionId.Clear();
            _itemByContentItemId.Clear();
            _publishedItemsById.Clear();
        }
    }
}