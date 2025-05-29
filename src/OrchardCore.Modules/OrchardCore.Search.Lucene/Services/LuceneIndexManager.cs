using System.Collections.Concurrent;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.Services;
using Spatial4n.Context;
using Directory = System.IO.Directory;
using LDirectory = Lucene.Net.Store.Directory;
using LuceneLock = Lucene.Net.Store.Lock;

namespace OrchardCore.Search.Lucene;

/// <summary>
/// Provides methods to manage physical Lucene indices.
/// This class is provided as a singleton to that the index searcher can be reused across requests.
/// </summary>
public sealed class LuceneIndexManager : IIndexManager, IIndexDocumentManager, IDisposable
{
    private readonly IClock _clock;
    private readonly LuceneIndexingState _luceneIndexingState;
    private readonly ILogger _logger;
    private readonly string _rootPath;
    private bool _disposed;
    private readonly ConcurrentDictionary<string, IndexReaderPool> _indexPools = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IndexWriterWrapper> _writers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly IEnumerable<IIndexEvents> _indexEvents;
    private readonly SpatialContext _ctx;
    private readonly GeohashPrefixTree _grid;
    private static readonly object _synLock = new();

    public LuceneIndexManager(
        IClock clock,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        LuceneIndexingState luceneIndexingState,
        ILogger<LuceneIndexManager> logger,
        LuceneAnalyzerManager luceneAnalyzerManager,
        IEnumerable<IIndexEvents> indexEvents
        )
    {
        _clock = clock;
        _luceneIndexingState = luceneIndexingState;
        _logger = logger;
        _rootPath = PathExtensions.Combine(
            shellOptions.Value.ShellsApplicationDataPath,
            shellOptions.Value.ShellsContainerName,
            shellSettings.Name, "Lucene");
        Directory.CreateDirectory(_rootPath);
        _luceneAnalyzerManager = luceneAnalyzerManager;
        _indexEvents = indexEvents;

        // Typical geospatial context.
        // These can also be constructed from SpatialContextFactory.
        _ctx = SpatialContext.Geo;

        var maxLevels = 11; // Results in sub-meter precision for geohash.

        // TODO demo lookup by detail distance.
        // This can also be constructed from SpatialPrefixTreeFactory.
        _grid = new GeohashPrefixTree(_ctx, maxLevels);
    }

    public async Task<bool> CreateAsync(IndexEntity index)
    {
        if (await ExistsAsync(index.IndexFullName))
        {
            return false;
        }

        var context = new IndexCreateContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

        try
        {
            await WriteAsync(index, _ => { }, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), context, _logger);

        return true;
    }

    public async Task<bool> RebuildAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var context = new IndexRebuildContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

        if (await ExistsAsync(index.IndexFullName))
        {
            RemoveIndexInternal(index.IndexFullName);
        }

        try
        {
            await WriteAsync(index, _ => { }, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

        return true;
    }

    public async Task<bool> DeleteAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var context = new IndexRemoveContext(indexFullName);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);
        try
        {
            RemoveIndexInternal(indexFullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing index: {IndexFullName}", indexFullName);

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

        return true;
    }

    public Task<bool> ExistsAsync(string indexFullName)
    {
        if (string.IsNullOrWhiteSpace(indexFullName))
        {
            return Task.FromResult(false);
        }

        try
        {
            var exists = Directory.Exists(PathExtensions.Combine(_rootPath, indexFullName));

            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of index: {IndexFullName}", indexFullName);

            return Task.FromResult(false);
        }
    }

    public async Task<bool> DeleteDocumentsAsync(IndexEntity index, IEnumerable<string> documentIds)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(documentIds);

        if (!documentIds.Any())
        {
            return false;
        }

        await WriteAsync(index, writer =>
        {
            var metadata = index.As<LuceneIndexMetadata>();

            writer.DeleteDocuments(documentIds.Select(x => new Term(metadata.IndexMappings.KeyFieldName, x)).ToArray());

            writer.Commit();

            if (_indexPools.TryRemove(index.IndexFullName, out var pool))
            {
                pool.MakeDirty();
                pool.Release();
            }
        });

        return true;
    }

    public async Task<bool> MergeOrUploadDocumentsAsync(IndexEntity index, IEnumerable<DocumentIndexBase> documents)
    {
        ArgumentNullException.ThrowIfNull(index);

        var metadata = index.As<LuceneContentIndexMetadata>();

        try
        {
            await WriteAsync(index, writer =>
            {
                foreach (var indexDocument in documents)
                {
                    writer.AddDocument(CreateLuceneDocument(indexDocument, metadata.StoreSourceData));
                }

                writer.Commit();

                if (_indexPools.TryRemove(index.IndexFullName, out var pool))
                {
                    pool.MakeDirty();
                    pool.Release();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging or uploading documents to index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAllDocumentsAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        try
        {
            await WriteAsync(index, writer =>
            {
                writer.DeleteAll();
                writer.Commit();

                if (_indexPools.TryRemove(index.IndexFullName, out var pool))
                {
                    pool.MakeDirty();
                    pool.Release();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all documents from index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        return true;
    }

    public Task<long> GetLastTaskIdAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        return Task.FromResult(_luceneIndexingState.GetLastTaskId(index.IndexFullName));
    }

    public Task SetLastTaskIdAsync(IndexEntity index, long lastTaskId)
    {
        ArgumentNullException.ThrowIfNull(index);

        _luceneIndexingState.SetLastTaskId(index.IndexFullName, lastTaskId);
        _luceneIndexingState.Update();

        return Task.CompletedTask;
    }

    public IContentIndexSettings GetContentIndexSettings()
         => new LuceneContentIndexSettings();

    public async Task SearchAsync(IndexEntity index, Func<IndexSearcher, Task> searcher)
    {
        if (await ExistsAsync(index.IndexFullName))
        {
            using (var reader = GetReader(index.IndexFullName))
            {
                var indexSearcher = new IndexSearcher(reader.IndexReader);
                await searcher(indexSearcher);
            }

            _timestamps[index.IndexFullName] = _clock.UtcNow;
        }
    }

    private void RemoveIndexInternal(string indexFullName)
    {
        lock (this)
        {
            if (_writers.TryGetValue(indexFullName, out var writer))
            {
                writer.IsClosing = true;
                writer.Dispose();
            }

            if (_indexPools.TryRemove(indexFullName, out var reader))
            {
                reader.Dispose();
            }

            _timestamps.TryRemove(indexFullName, out _);

            var indexFolder = PathExtensions.Combine(_rootPath, indexFullName);

            if (Directory.Exists(indexFolder))
            {
                try
                {
                    Directory.Delete(indexFolder, true);
                }
                catch { }
            }

            _writers.TryRemove(indexFullName, out _);
        }
    }

    private Document CreateLuceneDocument(DocumentIndexBase document, bool storeSourceData)
    {
        var doc = new Document();

        if (document is DocumentIndex documentIndex)
        {
            // Always store the content item id and version id.
            // These fields need to be indexed as a StringField because it needs to be searchable for the writer.DeleteDocuments method.
            // Else it won't be able to prune oldest draft from the indexes.
            // Maybe eventually find a way to remove a document from a StoredDocument.
            doc.Add(new StringField(ContentIndexingConstants.ContentItemIdKey, documentIndex.ContentItemId.ToString(), Field.Store.YES));
            doc.Add(new StringField(ContentIndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId?.ToString(), Field.Store.YES));

            if (storeSourceData)
            {
                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + ContentIndexingConstants.ContentItemIdKey, documentIndex.ContentItemId.ToString()));
                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + ContentIndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId.ToString()));
            }
        }

        foreach (var entry in document.Entries)
        {
            var store = entry.Options.HasFlag(DocumentIndexOptions.Store)
                        ? Field.Store.YES
                        : Field.Store.NO;

            switch (entry.Type)
            {
                case DocumentIndexBase.Types.Boolean:
                    // Store "true"/"false" for boolean.
                    doc.Add(new StringField(entry.Name, Convert.ToString(entry.Value).ToLowerInvariant(), store));

                    if (storeSourceData)
                    {
                        doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, Convert.ToString(entry.Value).ToLowerInvariant()));
                    }
                    break;

                case DocumentIndexBase.Types.DateTime:
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

                        if (storeSourceData)
                        {
                            if (entry.Value is DateTimeOffset)
                            {
                                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, DateTools.DateToString(((DateTimeOffset)entry.Value).UtcDateTime, DateResolution.SECOND)));
                            }
                            else
                            {
                                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, DateTools.DateToString(((DateTime)entry.Value).ToUniversalTime(), DateResolution.SECOND)));
                            }
                        }
                    }
                    else
                    {
                        doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                    }
                    break;

                case DocumentIndexBase.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        doc.Add(new Int64Field(entry.Name, value, store));

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, value));
                        }
                    }
                    else
                    {
                        doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                    }

                    break;

                case DocumentIndexBase.Types.Number:
                    if (entry.Value != null)
                    {
                        doc.Add(new DoubleField(entry.Name, Convert.ToDouble(entry.Value), store));

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, Convert.ToDouble(entry.Value)));
                        }
                    }
                    else
                    {
                        doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                    }
                    break;

                case DocumentIndexBase.Types.Text:
                    if (entry.Value != null && !string.IsNullOrEmpty(Convert.ToString(entry.Value)))
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

                        // This is for Elasticsearch Queries compatibility since a keyword field is always indexed
                        // by default when indexing without explicit mapping in Elasticsearch.
                        // Keyword ignore above 256 chars by default.
                        if (store == Field.Store.NO && !entry.Options.HasFlag(DocumentIndexOptions.Keyword) && stringValue.Length <= 256)
                        {
                            doc.Add(new StringField($"{entry.Name}.keyword", stringValue, Field.Store.NO));
                        }

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, stringValue));
                        }
                    }
                    else
                    {
                        if (entry.Options.HasFlag(DocumentIndexOptions.Keyword))
                        {
                            doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                        }
                        else
                        {
                            doc.Add(new TextField(entry.Name, ContentIndexingConstants.NullValue, store));
                        }
                    }
                    break;

                case DocumentIndexBase.Types.GeoPoint:
                    var strategy = new RecursivePrefixTreeStrategy(_grid, entry.Name);

                    if (entry.Value != null && entry.Value is DocumentIndexBase.GeoPoint point)
                    {
                        var geoPoint = _ctx.MakePoint((double)point.Longitude, (double)point.Latitude);
                        foreach (var field in strategy.CreateIndexableFields(geoPoint))
                        {
                            doc.Add(field);
                        }

                        doc.Add(new StoredField(strategy.FieldName, $"{point.Latitude},{point.Longitude}"));

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + strategy.FieldName, $"{point.Latitude},{point.Longitude}"));
                        }
                    }
                    else
                    {
                        doc.Add(new StoredField(strategy.FieldName, ContentIndexingConstants.NullValue));
                    }
                    break;
            }
        }

        return doc;
    }

    private FSDirectory CreateDirectory(string indexName)
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

    private Task WriteAsync(IndexEntity index, Action<IndexWriter> action, bool close = false)
    {
        if (!_writers.TryGetValue(index.IndexFullName, out var writer))
        {
            var metadata = index.As<LuceneIndexMetadata>();
            var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();

            lock (this)
            {
                if (!_writers.TryGetValue(index.IndexFullName, out writer))
                {
                    var directory = CreateDirectory(index.IndexFullName);
                    var analyzer = _luceneAnalyzerManager.CreateAnalyzer(metadata.AnalyzerName ?? LuceneConstants.DefaultAnalyzer);
                    var config = new IndexWriterConfig(queryMetadata.DefaultVersion, analyzer)
                    {
                        OpenMode = OpenMode.CREATE_OR_APPEND,
                        WriteLockTimeout = LuceneLock.LOCK_POLL_INTERVAL * 3,
                    };

                    writer = new IndexWriterWrapper(directory, config);

                    if (close)
                    {
                        action?.Invoke(writer);
                        writer.Dispose();
                        _timestamps[index.IndexFullName] = _clock.UtcNow;
                        return Task.CompletedTask;
                    }

                    _writers[index.IndexFullName] = writer;
                }
            }
        }

        if (writer.IsClosing)
        {
            return Task.CompletedTask;
        }

        action?.Invoke(writer);

        _timestamps[index.IndexFullName] = _clock.UtcNow;

        return Task.CompletedTask;
    }

    private IndexReaderPool.IndexReaderLease GetReader(string indexFullName)
    {
        var pool = _indexPools.GetOrAdd(indexFullName, n =>
        {
            var path = new DirectoryInfo(PathExtensions.Combine(_rootPath, indexFullName));
            var reader = DirectoryReader.Open(FSDirectory.Open(path));
            return new IndexReaderPool(reader);
        });

        return pool.Acquire();
    }

    /// <summary>
    /// Releases all readers and writers. This can be used after some time of inactivity to free resources.
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

internal sealed class IndexWriterWrapper : IndexWriter
{
    public IndexWriterWrapper(LDirectory directory, IndexWriterConfig config) : base(directory, config)
    {
        IsClosing = false;
    }

    public bool IsClosing { get; set; }
}

internal sealed class IndexReaderPool : IDisposable
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
