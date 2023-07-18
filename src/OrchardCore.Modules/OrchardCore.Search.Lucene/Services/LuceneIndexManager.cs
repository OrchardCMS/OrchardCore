using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Services;
using Spatial4n.Context;
using Directory = System.IO.Directory;
using LDirectory = Lucene.Net.Store.Directory;

namespace OrchardCore.Search.Lucene
{
    /// <summary>
    /// Provides methods to manage physical Lucene indices.
    /// This class is provided as a singleton to that the index searcher can be reused across requests.
    /// </summary>
    public class LuceneIndexManager : IDisposable
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly string _rootPath;
        private bool _disposed;
        private readonly ConcurrentDictionary<string, IndexReaderPool> _indexPools = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IndexWriterWrapper> _writers = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly SpatialContext _ctx;
        private readonly GeohashPrefixTree _grid;
        private readonly static object _synLock = new();

        public LuceneIndexManager(
            IClock clock,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ILogger<LuceneIndexManager> logger,
            LuceneAnalyzerManager luceneAnalyzerManager,
            LuceneIndexSettingsService luceneIndexSettingsService
            )
        {
            _clock = clock;
            _logger = logger;
            _rootPath = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name, "Lucene");
            Directory.CreateDirectory(_rootPath);
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _luceneIndexSettingsService = luceneIndexSettingsService;

            // Typical geospatial context.
            // These can also be constructed from SpatialContextFactory.
            _ctx = SpatialContext.Geo;

            var maxLevels = 11; // Results in sub-meter precision for geohash.

            // TODO demo lookup by detail distance.
            // This can also be constructed from SpatialPrefixTreeFactory.
            _grid = new GeohashPrefixTree(_ctx, maxLevels);
        }

        public async Task CreateIndexAsync(string indexName)
        {
            await WriteAsync(indexName, _ => { }, true);
        }

        public async Task DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
        {
            await WriteAsync(indexName, writer =>
            {
                writer.DeleteDocuments(contentItemIds.Select(x => new Term("ContentItemId", x)).ToArray());

                writer.Commit();

                if (_indexPools.TryRemove(indexName, out var pool))
                {
                    pool.MakeDirty();
                    pool.Release();
                }
            });
        }

        public void DeleteIndex(string indexName)
        {
            lock (this)
            {
                if (_writers.TryGetValue(indexName, out var writer))
                {
                    writer.IsClosing = true;
                    writer.Dispose();
                }

                if (_indexPools.TryRemove(indexName, out var reader))
                {
                    reader.Dispose();
                }

                _timestamps.TryRemove(indexName, out _);

                var indexFolder = PathExtensions.Combine(_rootPath, indexName);

                if (Directory.Exists(indexFolder))
                {
                    try
                    {
                        Directory.Delete(indexFolder, true);
                    }
                    catch { }
                }

                _writers.TryRemove(indexName, out _);
            }
        }

        public bool Exists(string indexName)
        {
            if (String.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }

            return Directory.Exists(PathExtensions.Combine(_rootPath, indexName));
        }

        public async Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(indexName);

            await WriteAsync(indexName, writer =>
            {
                foreach (var indexDocument in indexDocuments)
                {
                    writer.AddDocument(CreateLuceneDocument(indexDocument, luceneIndexSettings));
                }

                writer.Commit();

                if (_indexPools.TryRemove(indexName, out var pool))
                {
                    pool.MakeDirty();
                    pool.Release();
                }
            });
        }

        public async Task SearchAsync(string indexName, Func<IndexSearcher, Task> searcher)
        {
            if (Exists(indexName))
            {
                using (var reader = GetReader(indexName))
                {
                    var indexSearcher = new IndexSearcher(reader.IndexReader);
                    await searcher(indexSearcher);
                }

                _timestamps[indexName] = _clock.UtcNow;
            }
        }

        public void Read(string indexName, Action<IndexReader> reader)
        {
            if (Exists(indexName))
            {
                using (var indexReader = GetReader(indexName))
                {
                    reader(indexReader.IndexReader);
                }

                _timestamps[indexName] = _clock.UtcNow;
            }
        }

        /// <summary>
        /// Returns a list of open indices and the last time they were accessed.
        /// </summary>
        public IReadOnlyDictionary<string, DateTime> GetTimestamps()
        {
            return new ReadOnlyDictionary<string, DateTime>(_timestamps);
        }

        private Document CreateLuceneDocument(DocumentIndex documentIndex, LuceneIndexSettings indexSettings)
        {
            var doc = new Document
            {
                // Always store the content item id and version id.
                // These fields need to be indexed as a StringField because it needs to be searchable for the writer.DeleteDocuments method.
                // Else it won't be able to prune oldest draft from the indexes.
                // Maybe eventually find a way to remove a document from a StoredDocument.
                new StringField("ContentItemId", documentIndex.ContentItemId.ToString(), Field.Store.YES),
                new StringField("ContentItemVersionId", documentIndex.ContentItemVersionId.ToString(), Field.Store.YES),
            };

            if (indexSettings.StoreSourceData)
            {
                doc.Add(new StoredField(IndexingConstants.SourceKey + "ContentItemId", documentIndex.ContentItemId.ToString()));
                doc.Add(new StoredField(IndexingConstants.SourceKey + "ContentItemVersionId", documentIndex.ContentItemVersionId.ToString()));
            }

            foreach (var entry in documentIndex.Entries)
            {
                var store = entry.Options.HasFlag(DocumentIndexOptions.Store)
                            ? Field.Store.YES
                            : Field.Store.NO;

                switch (entry.Type)
                {
                    case DocumentIndex.Types.Boolean:
                        // Store "true"/"false" for booleans.
                        doc.Add(new StringField(entry.Name, Convert.ToString(entry.Value).ToLowerInvariant(), store));

                        if (indexSettings.StoreSourceData)
                        {
                            doc.Add(new StoredField(IndexingConstants.SourceKey + entry.Name, Convert.ToString(entry.Value).ToLowerInvariant()));
                        }
                        break;

                    case DocumentIndex.Types.DateTime:
                        if (entry.Value != null)
                        {
                            if (entry.Value is DateTimeOffset)
                            {
                                doc.Add(new StringField(entry.Name, DateTools.DateToString(((DateTimeOffset)entry.Value).UtcDateTime, DateResolution.SECOND), store));
                            }
                            else
                            {
                                doc.Add(new StringField(entry.Name, DateTools.DateToString(((DateTime)entry.Value).ToUniversalTime(), DateResolution.SECOND), store));
                            }

                            if (indexSettings.StoreSourceData)
                            {
                                if (entry.Value is DateTimeOffset)
                                {
                                    doc.Add(new StoredField(IndexingConstants.SourceKey + entry.Name, DateTools.DateToString(((DateTimeOffset)entry.Value).UtcDateTime, DateResolution.SECOND)));
                                }
                                else
                                {
                                    doc.Add(new StoredField(IndexingConstants.SourceKey + entry.Name, DateTools.DateToString(((DateTime)entry.Value).ToUniversalTime(), DateResolution.SECOND)));
                                }
                            }
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Name, "NULL", store));
                        }
                        break;

                    case DocumentIndex.Types.Integer:
                        if (entry.Value != null && Int64.TryParse(entry.Value.ToString(), out var value))
                        {
                            doc.Add(new Int64Field(entry.Name, value, store));

                            if (indexSettings.StoreSourceData)
                            {
                                doc.Add(new StoredField(IndexingConstants.SourceKey + entry.Name, value));
                            }
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Name, "NULL", store));
                        }

                        break;

                    case DocumentIndex.Types.Number:
                        if (entry.Value != null)
                        {
                            doc.Add(new DoubleField(entry.Name, Convert.ToDouble(entry.Value), store));

                            if (indexSettings.StoreSourceData)
                            {
                                doc.Add(new StoredField(IndexingConstants.SourceKey + entry.Name, Convert.ToDouble(entry.Value)));
                            }
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Name, "NULL", store));
                        }
                        break;

                    case DocumentIndex.Types.Text:
                        if (entry.Value != null && !String.IsNullOrEmpty(Convert.ToString(entry.Value)))
                        {
                            var stringValue = Convert.ToString(entry.Value);

                            if (entry.Options.HasFlag(DocumentIndexOptions.Keyword))
                            {
                                doc.Add(new StringField(entry.Name, stringValue, store));
                            }
                            else
                            {
                                doc.Add(new TextField(entry.Name, stringValue, store));
                            }

                            // This is for ElasticSearch Queries compatibility since a keyword field is always indexed
                            // by default when indexing without explicit mapping in ElasticSearch.
                            // Keyword ignore above 256 chars by default.
                            if (store == Field.Store.NO && !entry.Options.HasFlag(DocumentIndexOptions.Keyword) && stringValue.Length <= 256)
                            {
                                doc.Add(new StringField($"{entry.Name}.keyword", stringValue, Field.Store.NO));
                            }

                            if (indexSettings.StoreSourceData)
                            {
                                doc.Add(new StoredField(IndexingConstants.SourceKey + entry.Name, stringValue));
                            }
                        }
                        else
                        {
                            if (entry.Options.HasFlag(DocumentIndexOptions.Keyword))
                            {
                                doc.Add(new StringField(entry.Name, "NULL", store));
                            }
                            else
                            {
                                doc.Add(new TextField(entry.Name, "NULL", store));
                            }
                        }
                        break;

                    case DocumentIndex.Types.GeoPoint:
                        var strategy = new RecursivePrefixTreeStrategy(_grid, entry.Name);

                        if (entry.Value != null && entry.Value is DocumentIndex.GeoPoint point)
                        {
                            var geoPoint = _ctx.MakePoint((double)point.Longitude, (double)point.Latitude);
                            foreach (var field in strategy.CreateIndexableFields(geoPoint))
                            {
                                doc.Add(field);
                            }

                            doc.Add(new StoredField(strategy.FieldName, $"{point.Latitude},{point.Longitude}"));

                            if (indexSettings.StoreSourceData)
                            {
                                doc.Add(new StoredField(IndexingConstants.SourceKey + strategy.FieldName, $"{point.Latitude},{point.Longitude}"));
                            }
                        }
                        else
                        {
                            doc.Add(new StoredField(strategy.FieldName, "NULL"));
                        }
                        break;
                }
            }

            return doc;
        }

        private BaseDirectory CreateDirectory(string indexName)
        {
            lock (this)
            {
                var path = new DirectoryInfo(PathExtensions.Combine(_rootPath, indexName));

                if (!path.Exists)
                {
                    path.Create();
                }

                // Lucene is not thread safe on this call.
                lock (_synLock)
                {
                    return FSDirectory.Open(path);
                }
            }
        }

        private async Task WriteAsync(string indexName, Action<IndexWriter> action, bool close = false)
        {
            if (!_writers.TryGetValue(indexName, out var writer))
            {
                var indexAnalyzer = await _luceneIndexSettingsService.LoadIndexAnalyzerAsync(indexName);
                lock (this)
                {
                    if (!_writers.TryGetValue(indexName, out writer))
                    {
                        var directory = CreateDirectory(indexName);
                        var analyzer = _luceneAnalyzerManager.CreateAnalyzer(indexAnalyzer);
                        var config = new IndexWriterConfig(LuceneSettings.DefaultVersion, analyzer)
                        {
                            OpenMode = OpenMode.CREATE_OR_APPEND,
                            WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 3
                        };

                        writer = new IndexWriterWrapper(directory, config);

                        if (close)
                        {
                            action?.Invoke(writer);
                            writer.Dispose();
                            _timestamps[indexName] = _clock.UtcNow;
                            return;
                        }

                        _writers[indexName] = writer;
                    }
                }
            }

            if (writer.IsClosing)
            {
                return;
            }

            action?.Invoke(writer);

            _timestamps[indexName] = _clock.UtcNow;
        }

        private IndexReaderPool.IndexReaderLease GetReader(string indexName)
        {
            var pool = _indexPools.GetOrAdd(indexName, n =>
            {
                var path = new DirectoryInfo(PathExtensions.Combine(_rootPath, indexName));
                var reader = DirectoryReader.Open(FSDirectory.Open(path));
                return new IndexReaderPool(reader);
            });

            return pool.Acquire();
        }

        /// <summary>
        /// Releases all readers and writers. This can be used after some time of innactivity to free resources.
        /// </summary>
        public void FreeReaderWriter()
        {
            lock (this)
            {
                foreach (var writer in _writers)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Freeing writer for: '{IndexName}'.", writer.Key);
                    }

                    writer.Value.IsClosing = true;
                    writer.Value.Dispose();
                }

                foreach (var reader in _indexPools)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Freeing reader for: '{IndexName}'.", reader.Key);
                    }

                    reader.Value.Dispose();
                }

                _writers.Clear();
                _indexPools.Clear();
                _timestamps.Clear();
            }
        }

        /// <summary>
        /// Releases all readers and writers. This can be used after some time of innactivity to free resources.
        /// </summary>
        public void FreeReaderWriter(string indexName)
        {
            lock (this)
            {
                if (_writers.TryGetValue(indexName, out var writer))
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Freeing writer for: '{IndexName}'.", indexName);
                    }

                    writer.IsClosing = true;
                    writer.Dispose();
                }

                if (_indexPools.TryGetValue(indexName, out var reader))
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Freeing reader for: '{IndexName}'.", indexName);
                    }

                    reader.Dispose();
                }

                _timestamps.TryRemove(indexName, out _);

                _writers.TryRemove(indexName, out _);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            FreeReaderWriter();

            GC.SuppressFinalize(this);
        }

        ~LuceneIndexManager()
        {
            if (_disposed)
            {
                return;
            }

            FreeReaderWriter();
        }
    }

    internal class IndexWriterWrapper : IndexWriter
    {
        public IndexWriterWrapper(LDirectory directory, IndexWriterConfig config) : base(directory, config)
        {
            IsClosing = false;
        }

        public bool IsClosing { get; set; }
    }

    internal class IndexReaderPool : IDisposable
    {
        private bool _dirty;
        private int _count;
        private readonly IndexReader _reader;

        public IndexReaderPool(IndexReader reader)
        {
            _reader = reader;
        }

        public void MakeDirty()
        {
            _dirty = true;
        }

        public IndexReaderLease Acquire()
        {
            return new IndexReaderLease(this, _reader);
        }

        public void Release()
        {
            if (_dirty && _count == 0)
            {
                _reader.Dispose();
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public readonly struct IndexReaderLease : IDisposable
        {
            private readonly IndexReaderPool _pool;

            public IndexReaderLease(IndexReaderPool pool, IndexReader reader)
            {
                _pool = pool;
                Interlocked.Increment(ref _pool._count);
                IndexReader = reader;
            }

            public IndexReader IndexReader { get; }

            public void Dispose()
            {
                Interlocked.Decrement(ref _pool._count);
                _pool.Release();
            }
        }
    }
}
