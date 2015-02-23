using System;
using System.Collections.Generic;

namespace OrchardVNext.ContentManagement {
    public class DefaultContentManagerSession : IContentManagerSession {
        private readonly IDictionary<int, ContentItem> _itemByVersionRecordId = new Dictionary<int, ContentItem>();
        private readonly IDictionary<Tuple<int, int>, ContentItem> _itemByVersionNumber = new Dictionary<Tuple<int, int>, ContentItem>();
        private readonly IDictionary<int, ContentItem> _publishedItemsByContentRecordId = new Dictionary<int, ContentItem>();

        public void Store(ContentItem item) {
            _itemByVersionRecordId.Add(item.VersionRecord.Id, item);
            _itemByVersionNumber.Add(Tuple.Create(item.Id, item.Version), item);

            // is it the Published version ?
            if (item.VersionRecord.Latest && item.VersionRecord.Published) {
                _publishedItemsByContentRecordId[item.Id] = item;
            }
        }

        public bool RecallVersionRecordId(int id, out ContentItem item) {
            return _itemByVersionRecordId.TryGetValue(id, out item);
        }

        public bool RecallVersionNumber(int id, int version, out ContentItem item) {
            return _itemByVersionNumber.TryGetValue(Tuple.Create(id, version), out item);
        }

        public bool RecallContentRecordId(int id, out ContentItem item) {
            return _publishedItemsByContentRecordId.TryGetValue(id, out item);
        }

        public void Clear() {
            _itemByVersionRecordId.Clear();
            _itemByVersionNumber.Clear();
            _publishedItemsByContentRecordId.Clear();
        }
    }
}