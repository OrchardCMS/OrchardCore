using System;
using System.Collections.Generic;

namespace Orchard.ContentManagement
{
    public class DefaultContentManagerSession : IContentManagerSession
    {
        private readonly Dictionary<int, ContentItem> _itemByVersionId = new Dictionary<int, ContentItem>();
        private readonly Dictionary<Tuple<string, int>, ContentItem> _itemByContentItemId = new Dictionary<Tuple<string, int>, ContentItem>();
        private readonly Dictionary<string, ContentItem> _publishedItemsById = new Dictionary<string, ContentItem>();

        private bool _hasItems;

        public void Store(ContentItem item)
        {
            _hasItems = true;

            _itemByVersionId.Add(item.Id, item);
            _itemByContentItemId.Add(Tuple.Create(item.ContentItemId, item.Number), item);

            // is it the  Published version ?
            if (item.Published)
            {
                _publishedItemsById[item.ContentItemId] = item;
            }
        }

        public bool RecallVersionId(int id, out ContentItem item)
        {
            if (!_hasItems)
            {
                item = null;
                return false;
            }

            return _itemByVersionId.TryGetValue(id, out item);
        }

        public bool RecallContentItemId(string contentItemId, int versionNumber, out ContentItem item)
        {
            if (!_hasItems)
            {
                item = null;
                return false;
            }

            return _itemByContentItemId.TryGetValue(Tuple.Create(contentItemId, versionNumber), out item);
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
            _itemByContentItemId.Clear();
            _publishedItemsById.Clear();
            _hasItems = false;
        }
    }
}