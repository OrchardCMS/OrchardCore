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

        public void Store(ContentItem contentItem) {
            _dataContext.Add(contentItem.Record);
            _dataContext.Add(contentItem.VersionRecord);
        }

        public ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public ContentItem Get(int id, VersionOptions options) {
            var record = _dataContext.Set<ContentItemRecord>().FirstOrDefault(x => x.Id == id);

            return new ContentItem { VersionRecord = GetVersionRecord(options, record) };
        }

        public IEnumerable<ContentItem> GetMany(IEnumerable<int> ids) {
            return _dataContext.Set<ContentItemVersionRecord>().Where(x => ids.Contains(x.Id)).Select(x => new ContentItem {
                VersionRecord = x
            });
        }

        private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord) {
            if (options.IsPublished) {
                return itemRecord.Versions.FirstOrDefault(x => x.Published);
            }
            if (options.IsLatest || options.IsDraftRequired) {
                return itemRecord.Versions.FirstOrDefault(x => x.Latest);
            }
            if (options.IsDraft) {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Latest && !x.Published);
            }
            if (options.VersionNumber != 0) {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Number == options.VersionNumber);
            }
            return null;
        }
    }
}