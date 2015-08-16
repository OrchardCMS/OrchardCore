using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement.Data {
    public class ContentItemStore : IContentItemStore {
        private readonly IContentStorageManager _contentStorageManager;

        public ContentItemStore(
            IContentStorageManager contentStorageManager) {
            _contentStorageManager = contentStorageManager;
        }

        private readonly Func<ContentItemVersionRecord, int, VersionOptions, bool> _query = (versionRecord, id, options) => {
            if (options.IsPublished) {
                return versionRecord.ContentItemRecord.Id == id && versionRecord.Published;
            }
            if (options.IsLatest || options.IsDraftRequired) {
                return versionRecord.ContentItemRecord.Id == id && versionRecord.Latest;
            }
            if (options.IsDraft) {
                return versionRecord.ContentItemRecord.Id == id && versionRecord.Latest && !versionRecord.Published;
            }
            if (options.VersionNumber != 0) {
                return versionRecord.ContentItemRecord.Id == id && versionRecord.Number == options.VersionNumber;
            }
            return versionRecord.ContentItemRecord.Id == id;
        };

        public void Store(ContentItem contentItem) {
            _contentStorageManager.Store(contentItem.Record);
            _contentStorageManager.Store(contentItem.VersionRecord);
        }

        public ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public ContentItem Get(int id, VersionOptions options) {
            var record = _contentStorageManager
                .Query<ContentItemVersionRecord>(x => _query(x, id, options))
                .OrderBy(x => x.Number)
                .LastOrDefault();

            return new ContentItem { VersionRecord = record };
        }

        public IReadOnlyList<ContentItem> GetMany(IEnumerable<int> ids) {
            return _contentStorageManager
                .Query<ContentItemVersionRecord>(x => ids.Contains(x.Id))
                .Select(x => new ContentItem { VersionRecord = x })
                .ToList();
        }
    }
}