using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            _contentIndexProvider.DeIndex(document);
        }

        public IEnumerable<T> Query<T>() where T : DocumentRecord {
            return _contentIndexProvider.Query<T>();
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> filter) where T : DocumentRecord {
            return _contentIndexProvider.Query(filter);
        }
    }
}