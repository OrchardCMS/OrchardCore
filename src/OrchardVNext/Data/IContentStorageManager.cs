using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OrchardVNext.Data {
    public interface IContentStorageManager : IDependency {
        TDocument Get<TDocument>(int id) where TDocument : StorageDocument;
        //IQueryable<TDocument> Query<TDocument>() where TDocument : StorageDocument;
        IEnumerable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
        void Store<TDocument>(TDocument document) where TDocument : StorageDocument;
        IEnumerable<TDocument> GetMany<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument;
    }

    public class DefaultContentStorageManager : IContentStorageManager {
        private readonly IEnumerable<IContentStore> _contentStores;
        private readonly IContentQueryStore _contentQueryStore;

        public DefaultContentStorageManager(
            IEnumerable<IContentStore> contentStores,
            IContentQueryStore contentQueryStore) {
            _contentStores = contentStores;
            _contentQueryStore = contentQueryStore;
        }

        public TDocument Get<TDocument>(int id) where TDocument : StorageDocument {
            foreach (var contentStore in _contentStores) {
                var result = contentStore.Get<TDocument>(id);
                if (result != null)
                    return result;
            }
            return null;
        }

        public IEnumerable<TDocument> GetMany<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument {
            throw new NotImplementedException();
        }

        public IEnumerable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
            return
                _contentQueryStore.Query<TDocument>(map).Records;
        }
        
        public void Store<TDocument>(TDocument document) where TDocument : StorageDocument {
            foreach (var contentStore in _contentStores) {
                contentStore.Store(document);
                _contentQueryStore.Index(document, contentStore.GetType());
            }
        }
    }

    public abstract class StorageDocument {
        public int Id { get; set; }
        public abstract object Data { get; set; }
    }

    public interface IContentStore : IDependency {
        TDocument Get<TDocument>(int id) where TDocument : StorageDocument;
        void Store<TDocument>(TDocument document) where TDocument : StorageDocument;
        IEnumerable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
    }

    public interface IContentQueryStore : IDependency {
        void Index<TDocument>(TDocument document, Type contentStore) where TDocument : StorageDocument;
        ContentIndexResult<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
    }

    public class ContentIndexResult<TDocument> {
        public IEnumerable<TDocument> Records { get; set; }

        public IEnumerable<TDocument> Reduce(Func<TDocument, bool> reduce) {
            return Records.Where(reduce);
        }
    }
}