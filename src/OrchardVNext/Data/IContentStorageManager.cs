using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nito.AsyncEx;

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
            var tasks = _contentStores
                .Select(s => s.Get<TDocument>(id))
                .OrderByCompletion();

            foreach (var task in tasks) {
                task.Start();
            }

            TDocument result = null;

            foreach (var task in tasks) {
                if (task.Result != null) {
                    result = task.Result;
                    break;
                }
            }

            return null;
        }

        public IEnumerable<TDocument> GetMany<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument {
            var tasks = _contentStores
                .Select(s => s.GetMany<TDocument>(ids))
                .OrderByCompletion();

            foreach (var task in tasks) {
                task.Start();
            }

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
                contentStore.Store(document).ContinueWith(x => {
                    if (x.Result > 0)
                        _contentQueryStore.Index(document, contentStore.GetType());
                }).Start();
            }
        }
    }

    public abstract class StorageDocument {
        public int Id { get; set; }
        public abstract object Data { get; set; }
    }

    public interface IContentStore : IDependency {
        Task<TDocument> Get<TDocument>(int id) where TDocument : StorageDocument;
        Task<int> Store<TDocument>(TDocument document) where TDocument : StorageDocument;
        Task<IEnumerable<TDocument>> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
        Task<IEnumerable<TDocument>> GetMany<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument;
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