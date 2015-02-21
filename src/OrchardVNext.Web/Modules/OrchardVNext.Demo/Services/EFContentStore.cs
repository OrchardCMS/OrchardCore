using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data;

namespace OrchardVNext.Demo.Services {
    public class EFContentStore : IContentStore {
        private readonly DataContext _dataContext;
        public EFContentStore(DataContext dataContext) {
            _dataContext = dataContext;
        }

        public void Store(ContentItem contentItem) {
            _dataContext.Add(contentItem.Record);
            _dataContext.Add(contentItem.VersionRecord);
            _dataContext.SaveChanges();
        }

        public ContentItem Get(int id) {
            var record = _dataContext.Set<ContentItemVersionRecord>().FirstOrDefault(x => x.Id == id);

            return new ContentItem {VersionRecord = record};
        }

        public IEnumerable<ContentItem> GetMany(IEnumerable<int> ids) {
            return _dataContext.Set<ContentItemVersionRecord>().Where(x => ids.Contains(x.Id)).Select(x => new ContentItem
            {
                VersionRecord = x
            });
        }
    }
}