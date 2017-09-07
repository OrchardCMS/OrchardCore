//using OrchardCore.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;
//using System.Linq.Expressions;
//using Microsoft.Extensions.DependencyInjection;
//using System.Threading.Tasks;
//#if DNXCORE50
//using System.Reflection;
//#endif

//namespace OrchardCore.Tests.Data {
//    public class DefaultContentStorageManagerTest {
//        protected IServiceProvider CreateContainer() {
//            var services = new ServiceCollection();
//            services.AddSingleton<IContentStore, FakeContentStore>();
//            services.AddSingleton<IContentQueryStore, FakeContentQueryStore>();

//            services.AddTransient<IContentStorageManager, DefaultContentStorageManager>();

//            return services.BuildServiceProvider();
//        }

//        [Fact]
//        public void ShouldStoreAndRetrieveDocumentById() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;

//            StorageDocument document = new FakeStorageDocument();
//            manager.Store(document);

//            var returnedDocument = manager.Get<FakeStorageDocument>(document.Id);

//            Assert.Equal(document.Id, returnedDocument.Id);
//        }

//        [Fact]
//        public void ShouldStoreThenStoreDocumentInIndex() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;

//            StorageDocument document = new FakeStorageDocument();
//            manager.Store(document);

//            var queryStore = container.GetService(typeof(IContentQueryStore)) as FakeContentQueryStore;
//            Assert.Equal(1, queryStore._documents.Count);
//            Assert.Equal(document.Id, queryStore._documents.Single().Id);

//            Assert.Equal(1, queryStore._indexCollection.Count);
//        }

//        [Fact]
//        public void ShouldStoreMultipleItemsAndIndexWithJustOneIndexCreated() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;

//            StorageDocument document1 = new FakeStorageDocument();
//            manager.Store(document1);

//            StorageDocument document2 = new FakeStorageDocument();
//            manager.Store(document2);

//            StorageDocument document3 = new FakeStorageDocument();
//            manager.Store(document3);

//            var queryStore = container.GetService(typeof(IContentQueryStore)) as FakeContentQueryStore;
//            Assert.Equal(3, queryStore._documents.Count);
//            Assert.Equal(1, queryStore._indexCollection.Count);
//        }

//        [Fact]
//        public void ShouldReturnNullRecordWhenIdDoesNotExist() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;

//            var returnedDocument = manager.Get<FakeStorageDocument>(0);

//            Assert.Null(returnedDocument);
//        }

//        [Fact]
//        public void ShouldNotIncrementTheDocumentId() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;
//            StorageDocument document = new FakeStorageDocument();
//            manager.Store(document);

//            int documentIdBefore = document.Id;
//            manager.Store(document);
//            int documentIdAfter = document.Id;

//            Assert.Equal(documentIdBefore, documentIdAfter);
//        }

//        [Fact]
//        public void ShoulMapByNumericComparison() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;
//            StorageDocument document = new FakeStorageDocument();
//            manager.Store(document);

//            var reducedRecord = manager.Query<FakeStorageDocument>(x => x.Id == document.Id).Single();

//            Assert.Equal(document.Id, reducedRecord.Id);
//        }

//        [Fact]
//        public void ShouldMapByNumericPropertyRange() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;
//            StorageDocument document1 = new FakeStorageDocument();
//            manager.Store(document1);

//            StorageDocument document2 = new FakeStorageDocument();
//            manager.Store(document2);

//            StorageDocument document3 = new FakeStorageDocument();
//            manager.Store(document3);

//            var reducedRecords = manager.Query<FakeStorageDocument>(x => x.Id >= document2.Id).ToList();

//            Assert.Equal(2, reducedRecords.Count);
//            Assert.Equal(document2.Id, reducedRecords.ElementAt(0).Id);
//            Assert.Equal(document3.Id, reducedRecords.ElementAt(1).Id);
//        }

//        [Fact]
//        public void ShouldReturnTheRecordFromTheFastestDataStore() {
//            Assert.False(true);
//        }

//        [Fact]
//        public void ShouldReturnNullWhenRecordIsRemoved() {
//            var container = CreateContainer();
//            var manager = container.GetService(typeof(IContentStorageManager)) as IContentStorageManager;

//            StorageDocument document = new FakeStorageDocument();
//            manager.Store(document);

//            var returnedDocument = manager.Get<FakeStorageDocument>(document.Id);

//            manager.Remove<FakeStorageDocument>(document.Id);

//            var returnedDocumentAfterRemove = manager.Get<FakeStorageDocument>(document.Id);

//            Assert.NotNull(returnedDocument);
//            Assert.Null(returnedDocumentAfterRemove);
//        }

//        //[Fact]
//        //public void ShouldFilterByNumericComparison() {
//        //    IContentStorageManager manager = new FakeContentStorageManager(new List<StorageDocument>());
//        //    StorageDocument document = new FakeStorageDocument();
//        //    manager.Store(document);

//        //    var returnedDocument = (from storageDocument in manager.Query<FakeStorageDocument>()
//        //        where storageDocument.Id == document.Id
//        //        select storageDocument).Single();

//        //    Assert.Equal(document.Id, returnedDocument.Id);
//        //}

//        //[Fact]
//        //public void ShouldFilterByNumericPropertyRange() {
//        //    IContentStorageManager manager = new FakeContentStorageManager(new List<StorageDocument>());
//        //    StorageDocument document = new FakeStorageDocument();
//        //    manager.Store(document);

//        //    StorageDocument document2 = new FakeStorageDocument();
//        //    manager.Store(document2);

//        //    StorageDocument document3 = new FakeStorageDocument();
//        //    manager.Store(document3);

//        //    var returnedDocuments = (from storageDocument in manager.Query<FakeStorageDocument>()
//        //                            where storageDocument.Id >= document2.Id
//        //                            select storageDocument);

//        //    Assert.Equal(2, returnedDocuments.Count());
//        //    Assert.Equal(document2.Id, returnedDocuments.ElementAt(0).Id);
//        //    Assert.Equal(document3.Id, returnedDocuments.ElementAt(1).Id);
//        //}

//        private class FakeStorageDocument : StorageDocument {
//            public override string Data {
//                get {
//                    throw new NotImplementedException();
//                }

//                set {
//                    throw new NotImplementedException();
//                }
//            }
//        }

//        public class FakeContentStore : IContentStore {
//            private readonly IList<StorageDocument> _documents = new List<StorageDocument>();

//            public async Task<TDocument> GetAsync<TDocument>(int id) where TDocument : StorageDocument {
//                return await Task.FromResult<TDocument>(_documents.SingleOrDefault(x => x.Id == id) as TDocument);
//            }

//            public async Task<IReadOnlyList<TDocument>> GetManyAsync<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument {
//                return await Task.FromResult<IReadOnlyList<TDocument>>(_documents
//                    .Where(x => x.GetType().Name == typeof(TDocument).Name)
//                    .Where(x => ids.Contains(x.Id))
//                    .Cast<TDocument>()
//                    .ToList());
//            }

//            public async Task<IReadOnlyList<TDocument>> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
//                return await Task.FromResult<IReadOnlyList<TDocument>>(_documents
//                    .Where(x => x.GetType().Name == typeof(TDocument).Name)
//                    .Cast<TDocument>()
//                    .Where(map.Compile())
//                    .ToList());
//            }

//            public Task RemoveAsync<TDocument>(int id) where TDocument : StorageDocument {
//                throw new NotImplementedException();
//            }

//            public async Task<int> StoreAsync<TDocument>(TDocument document) where TDocument : StorageDocument {
//                return await Task.FromResult<int>(Task.Run(() => {
//                    document.Id = _documents.Max(x => x.Id) + 1;
//                    _documents.Add(document);
//                    return document.Id;
//                }).Result);
//            }
//        }

//        public class FakeContentQueryStore : IContentQueryStore {
//            private readonly IEnumerable<IContentStore> _contentStores;
//            private readonly Lazy<IContentStorageManager> _contentStorageManager;

//            public readonly IList<InternalIndexCollection> _indexCollection = new List<InternalIndexCollection>();
//            public readonly IList<InternalIndexDocumentCollection> _documents = new List<InternalIndexDocumentCollection>();

//            public FakeContentQueryStore(IEnumerable<IContentStore> contentStores,
//                Lazy<IContentStorageManager> contentStorageManager) {
//                _contentStores = contentStores;
//                _contentStorageManager = contentStorageManager;
//            }

//            public ContentIndexResult<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
//                var indexName = map.Compile().ToString();
//                var type = typeof(TDocument);

//                if (!_indexCollection.Any(x => x.IndexName == indexName)) {
//                    CreateIndex(map);
//                }

//                var indexedRecords = _documents
//                    .Where(x => x.IndexName == indexName && x.Type == type);

//                return new ContentIndexResult<TDocument>() {
//                    Records = _contentStorageManager.Value.GetMany<TDocument>(indexedRecords.Select(x => x.Id)).ToList()
//                };
//            }

//            public void Index<TDocument>(TDocument document, Type contentStore) where TDocument : StorageDocument {
//                Expression<Func<TDocument, bool>> defaultIndex = x => x.Id > 0;

//                var indexName = defaultIndex.Compile().ToString();
//                var type = typeof(TDocument);

//                if (!_indexCollection.Any(x => x.IndexName == indexName)) {
//                    CreateIndex(defaultIndex);
//                }
//                else {
//                    var index = _indexCollection.Any(x => x.IndexName == indexName);

//                    var indexedDocument = _documents.SingleOrDefault(
//                        x => x.IndexName == indexName && x.ReferencedDocumentId == document.Id && x.Type == type);

//                    if (indexedDocument != null)
//                        _documents.Remove(indexedDocument);

//                    _documents.Add(new InternalIndexDocumentCollection {
//                        Id = _documents.Max(x => x.Id) + 1,
//                        IndexName = indexName,
//                        Type = type,
//                        ReferencedDocumentId = document.Id
//                    });
//                }
//            }

//            private void CreateIndex<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
//                var indexName = map.Compile().ToString();
//                var type = typeof(TDocument);

//                List<TDocument> documents = new List<TDocument>();
//                // TODO: Inject this in Lazy
//                foreach (var contentStore in _contentStores) {
//                    var result = contentStore.Query(map);
//                    if (result != null)
//                        documents.AddRange(result.Result);
//                }

//                for (int i = 0; i < documents.Count; i++) {
//                    _documents.Add(new InternalIndexDocumentCollection {
//                        Id = i,
//                        IndexName = indexName,
//                        Type = type,
//                        ReferencedDocumentId = documents[i].Id
//                    });
//                }

//                // create
//                _indexCollection.Add(new InternalIndexCollection {
//                    Id = _indexCollection.Max(x => x.Id) + 1,
//                    IndexName = indexName,
//                });
//            }

//            public void DeIndex<TDocument>(int id, Type contentStore) {
//                var type = typeof(TDocument);
//                var documents = _documents.Where(x => x.Type == type && x.ReferencedDocumentId == id).ToList();

//                foreach (var document in documents) {
//                    _documents.Remove(document);
//                }
//            }

//            public class InternalIndexCollection {
//                public int Id { get; set; }
//                public string IndexName { get; set; }
//            }

//            public class InternalIndexDocumentCollection {
//                public int Id { get; set; }
//                public string IndexName { get; set; }
//                public Type Type { get; set; }
//                public int ReferencedDocumentId { get; set; }
//            }
//        }

//        //public class FakeQueryable<TDocument> : IQueryable<TDocument> where TDocument : StorageDocument {
//        //    IQueryProvider _provider;
//        //    Expression _expression;

//        //    public FakeQueryable(IQueryProvider provider) {
//        //        if (provider == null) {
//        //            throw new ArgumentNullException("provider");
//        //        }
//        //        _provider = provider;
//        //        _expression = Expression.Constant(this);
//        //    }

//        //    public FakeQueryable(IQueryProvider provider, Expression expression) {
//        //        if (provider == null) {
//        //            throw new ArgumentNullException("provider");
//        //        }
//        //        if (expression == null) {
//        //            throw new ArgumentNullException("expression");
//        //        }
//        //        if (!typeof(IQueryable<TDocument>).IsAssignableFrom(expression.Type)) {
//        //            throw new ArgumentOutOfRangeException("expression");
//        //        }
//        //        _provider = provider;
//        //        _expression = expression;
//        //    }

//        //    public Type ElementType {
//        //        get { return typeof (TDocument); }
//        //    }

//        //    public Expression Expression {
//        //        get {
//        //            return _expression;
//        //        }
//        //    }

//        //    public IQueryProvider Provider {
//        //        get { return _provider; }
//        //    }

//        //    public IEnumerator<TDocument> GetEnumerator() {
//        //        return ((IEnumerable<TDocument>)Provider.Execute(Expression)).GetEnumerator();
//        //    }

//        //    IEnumerator IEnumerable.GetEnumerator() {
//        //        return ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();
//        //    }
//        //}
//    }
//}