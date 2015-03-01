using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data {
    public class ContentStorageProvider : IContentStorageProvider {
        private readonly IContentDocumentStore _contentDocumentStore;
        private readonly IContentIndexProvider _contentIndexProvider;

        public ContentStorageProvider(IContentDocumentStore contentDocumentStore,
            IContentIndexProvider contentIndexProvider) {
            _contentDocumentStore = contentDocumentStore;
            _contentIndexProvider = contentIndexProvider;
        }

        public void Store<T>(T document) where T : DocumentRecord {
            _contentDocumentStore.Store(document);
            _contentIndexProvider.Index(document);
        }

        public void Remove<T>(T document) where T : DocumentRecord {
            _contentDocumentStore.Remove(document);
        }

        public IEnumerable<T> Query<T>() where T : DocumentRecord {
            var itemIds = _contentIndexProvider
                .Query<T>();

            return _contentDocumentStore.Query<T>(x => itemIds.Contains(x.Id));
        }

        public IEnumerable<T> Query<T>(Func<T, bool> filter) where T : DocumentRecord {
            var itemIds = _contentIndexProvider
                .Query(filter);

            return _contentDocumentStore.Query<T>(x => itemIds.Contains(x.Id));
        }
    }
}