using System;
using System.Collections.Generic;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Demo.Services {
    public class ContentStorageProvider : IContentStorageProvider {
        private readonly IContentStore _contentStore;
        private readonly IContentIndexProvider _contentIndexProvider;

        public ContentStorageProvider(IContentStore contentStore,
            IContentIndexProvider contentIndexProvider) {
            _contentStore = contentStore;
            _contentIndexProvider = contentIndexProvider;
        }

        public void Store(ContentItem contentItem) {
            _contentStore.Store(contentItem);
            _contentIndexProvider.Index(contentItem);
        }

        public IContent Get(int id) {
            return _contentStore.Get(id);
        }

        public IEnumerable<IContent> GetMany(Func<IContent, bool> filter) {
            var indexedItemIds = _contentIndexProvider.GetByFilter(filter);
            return _contentStore.GetMany(indexedItemIds);
        }
    }
}