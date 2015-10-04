using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Orchard.Data.EntityFramework {
    [OrchardFeature("Orchard.Data.EntityFramework.Indexing")]
    public class EFContentQueryStore : IContentQueryStore {
        private readonly IEnumerable<IContentStore> _contentStores;
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;

        private IQueryable<InternalIndexCollection> _indexCollection {
            get { return _dataContext.Set<InternalIndexCollection>().AsQueryable(); }
        }

        private IQueryable<InternalIndexDocumentCollection> _documents {
            get { return _dataContext.Set<InternalIndexDocumentCollection>().AsQueryable(); }
        }

        public EFContentQueryStore(IEnumerable<IContentStore> contentStores,
            ILoggerFactory loggerFactory,
            DataContext dataContext) {
            _contentStores = contentStores;
            _logger = loggerFactory.CreateLogger<EFContentQueryStore>();
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
                Records = _contentStores.Select(x => x.GetManyAsync<TDocument>(ids).Result).SelectMany(x => x).ToList()
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
                var indexedDocumentExists = _documents.Any(
                    x => x.IndexName == indexName && 
                    x.ReferencedDocumentId == document.Id && 
                    x.Type == type.ToString());

                if (!indexedDocumentExists) {
                    _dataContext
                        .Add(new InternalIndexDocumentCollection {
                            IndexName = indexName,
                            Type = type.ToString(),
                            ReferencedDocumentId = document.Id
                        });
                }
            }

            _dataContext.SaveChanges();
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
                        _dataContext.SaveChanges();
                    }
                }
            }

            if (documents.Count == 0) {
                _logger.LogInformation("[{0}]: No records for index: {1} - so skipping...", type.Name, indexName);
                return;
            }

            _logger.LogInformation("[{0}]: {1} records to add to new index: {2}", type.Name, documents.Count, indexName);

            _dataContext.AddRange(documents);

            // create 
            _dataContext
                .Add(new InternalIndexCollection {
                    IndexName = indexName,
                    Type = type.ToString()
                });

            _dataContext.SaveChanges();
        }

        public void DeIndex<TDocument>(int id, Type contentStore) {
            var type = typeof(TDocument);
            var documents = _documents.Where(x => x.Type == type.ToString() && x.ReferencedDocumentId == id).ToList();

            foreach (var document in documents) {
                _dataContext
                    .Remove(document);
            }
        }

        public class InternalIndexCollection : IndexDocument {
            public string IndexName { get; set; }
            public string Type { get; set; }
        }

        public class InternalIndexDocumentCollection : IndexDocument {
            public string IndexName { get; set; }
            public string Type { get; set; }
            public int ReferencedDocumentId { get; set; }
        }

        public class IndexDocument : StorageDocument {
            public override string Data { get; set; }
        }
    }
}