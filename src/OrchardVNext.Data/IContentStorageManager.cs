using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nito.AsyncEx;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Data {
    public interface IContentStorageManager : IDependency {
        TDocument Get<TDocument>(int id) where TDocument : StorageDocument;
        //IQueryable<TDocument> Query<TDocument>() where TDocument : StorageDocument;
        IEnumerable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
        void Store<TDocument>(TDocument document) where TDocument : StorageDocument;
        IEnumerable<TDocument> GetMany<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument;
        void Remove<TDocument>(int id) where TDocument : StorageDocument;
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
            var tasks = _contentStores
                .Select(s => s.GetAsync<TDocument>(id))
                .OrderByCompletion();
            
            TDocument result = null;

            foreach (var task in tasks) {
                if (task.Result != null) {
                    result = task.Result;
                    break;
                }
            }

            return result;
        }

        public IEnumerable<TDocument> GetMany<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument {
            var tasks = _contentStores
                .Select(s => s.GetManyAsync<TDocument>(ids))
                .OrderByCompletion();
            
            List<TDocument> results = new List<TDocument>();

            foreach (var task in tasks) {
                if (task.Result != null) {
                    results.AddRange(task.Result);
                    break;
                }
            }

            return results;
        }

        public IEnumerable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
            return
                _contentQueryStore.Query<TDocument>(map).Records;
        }
        
        public void Store<TDocument>(TDocument document) where TDocument : StorageDocument {
            foreach (var contentStore in _contentStores) {
                contentStore.StoreAsync(document).ContinueWith(x => {
                    if (x.Result > 0)
                        _contentQueryStore.Index(document, contentStore.GetType());
                });
            }
        }

        public void Remove<TDocument>(int id) where TDocument : StorageDocument {
            foreach (var contentStore in _contentStores) {
                contentStore.RemoveAsync<TDocument>(id).ContinueWith(x => {
                    _contentQueryStore.DeIndex<TDocument>(id, contentStore.GetType());
                });
            }
        }
    }
}