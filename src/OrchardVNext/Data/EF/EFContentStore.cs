using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data.EF {
    public class EFContentStore : IContentStore {
        private readonly DataContext _dataContext;
        public EFContentStore(DataContext dataContext) {
            _dataContext = dataContext;
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
            _dataContext.Add(contentItem.Record);
            _dataContext.Add(contentItem.VersionRecord);
        }

        public ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public ContentItem Get(int id, VersionOptions options) {
            var record = _dataContext.Set<ContentItemVersionRecord>().FirstOrDefault(x => _query(x, id, options));

            return new ContentItem { VersionRecord = record };
        }

        public IEnumerable<ContentItem> GetMany(IEnumerable<int> ids) {
            return _dataContext.Set<ContentItemVersionRecord>().Where(x => ids.Contains(x.Id)).Select(x => new ContentItem {
                VersionRecord = x
            });
        }
    }
}