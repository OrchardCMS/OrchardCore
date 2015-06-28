using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace OrchardVNext.Data.EF {
    public class EFContentQueryStore : IContentQueryStore {
        private readonly IEnumerable<IContentStore> _contentStores;
        private readonly DataContext _dataContext;

        private IQueryable<InternalIndexCollection> _indexCollection {
            get { return _dataContext.Set<InternalIndexCollection>().AsQueryable(); }
        }

        private IQueryable<InternalIndexDocumentCollection> _documents {
            get { return _dataContext.Set<InternalIndexDocumentCollection>().AsQueryable(); }
        }

        public EFContentQueryStore(IEnumerable<IContentStore> contentStores,
            DataContext dataContext) {
            _contentStores = contentStores;
            _dataContext = dataContext;
        }

        public ContentIndexResult<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
            var indexName = map.ToString();
            var type = typeof(TDocument);

            if (!_indexCollection.Any(x => x.IndexName == indexName && x.Type == type.ToString())) {
                CreateIndex(map);
            }

            var indexedRecords = _documents
                .Where(x => x.IndexName == indexName && x.Type == type.ToString());

            var ids = indexedRecords.Select(x => x.ReferencedDocumentId);
            
            return new ContentIndexResult<TDocument>() {
                Records = _contentStores.Select(x => x.GetManyAsync<TDocument>(ids).Result).SelectMany(x => x)
            };
        }

        public void Index<TDocument>(TDocument document, Type contentStore) where TDocument : StorageDocument {
            Expression<Func<TDocument, bool>> defaultIndex = x => x != null;

            var indexName = defaultIndex.ToString();
            var type = typeof(TDocument);

            if (!_indexCollection.Any(x => x.IndexName == indexName && x.Type == type.ToString())) {
                CreateIndex(defaultIndex);
            }
            else {
                var index = _indexCollection.Any(x => x.IndexName == indexName && x.Type == type.ToString());

                var indexedDocument = _documents.SingleOrDefault(
                    x => x.IndexName == indexName && x.ReferencedDocumentId == document.Id && x.Type == type.ToString());

                if (indexedDocument != null) {
                    _dataContext
                        .Remove(indexedDocument);
                }

                _dataContext
                    .Add(new InternalIndexDocumentCollection {
                        IndexName = indexName,
                        Type = type.ToString(),
                        ReferencedDocumentId = document.Id
                    });
            }
        }

        private void CreateIndex<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
            var indexName = map.ToString();
            var type = typeof(TDocument);


            var documents = new List<InternalIndexDocumentCollection>();
            // TODO: Inject this in Lazy
            foreach (var contentStore in _contentStores) {
                var result = contentStore.Query(map);
                if (result != null) {
                    foreach (var document in result.Result) {
                        documents.Add(new InternalIndexDocumentCollection {
                            IndexName = indexName,
                            Type = type.ToString(),
                            ReferencedDocumentId = document.Id
                        });
                    }
                }
            }

            if (documents.Count == 0) {
                Logger.TraceInformation("[{0}]: No records for index: {1} - so skipping...", type.Name, indexName);
                return;
            }

            Logger.TraceInformation("[{0}]: {1} records to add to new index: {2}", type.Name, documents.Count, indexName);

            _dataContext.AddRange(documents);

            // create 
            _dataContext
                .Add(new InternalIndexCollection {
                    IndexName = indexName,
                    Type = type.ToString()
                });
        }

        public void DeIndex<TDocument>(int id, Type contentStore) {
            var type = typeof(TDocument);
            var documents = _documents.Where(x => x.Type == type.ToString() && x.ReferencedDocumentId == id).ToList();

            foreach (var document in documents) {
                _dataContext
                    .Remove(document);
            }
        }

        [Persistent]
        public class InternalIndexCollection {
            [Key]
            public int Id { get; set; }
            public string IndexName { get; set; }
            public string Type { get; set; }
        }

        [Persistent]
        public class InternalIndexDocumentCollection {
            [Key]
            public int Id { get; set; }
            public string IndexName { get; set; }
            public string Type { get; set; }
            public int ReferencedDocumentId { get; set; }
        }
    }
}