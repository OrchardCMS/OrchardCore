using System.Collections.Concurrent;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene;
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.Services;
using Directory = System.IO.Directory;
using LDirectory = Lucene.Net.Store.Directory;
using LuceneLock = Lucene.Net.Store.Lock;
using OpenMode = Lucene.Net.Index.OpenMode;
namespace OrchardCore.Lucene.Core;

public sealed class LuceneIndexStore : IDisposable
{
    private readonly ConcurrentDictionary<string, IndexReaderPool> _indexPools = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IndexWriterWrapper> _writers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

    private static readonly object _synLock = new();

    private readonly string _rootPath;
    private readonly IClock _clock;
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly ILogger<LuceneIndexStore> _logger;

    private bool _disposed;

    public LuceneIndexStore(
        IClock clock,
        ShellSettings shellSettings,
        IOptions<ShellOptions> shellOptions,
        LuceneAnalyzerManager luceneAnalyzerManager,
        ILogger<LuceneIndexStore> logger)
    {
        _clock = clock;
        _luceneAnalyzerManager = luceneAnalyzerManager;
        _logger = logger;
        _rootPath = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name, "Lucene");
        Directory.CreateDirectory(_rootPath);
    }

    public bool Exists(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        try
        {
            var path = GetFullPath(indexFullName);

            return Directory.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of index: {IndexFullName}", indexFullName);

            return false;
        }
    }

    public void Remove(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

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

            var indexFolder = GetFullPath(indexFullName);

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

    public async Task SearchAsync(IndexEntity index, Func<IndexSearcher, Task> searcher)
    {
        if (Exists(index.IndexFullName))
        {
            using (var reader = GetReader(index.IndexFullName))
            {
                var indexSearcher = new IndexSearcher(reader.IndexReader);
                await searcher(indexSearcher);
            }

            _timestamps[index.IndexFullName] = _clock.UtcNow;
        }
    }

    public Task WriteAndClose(IndexEntity index)
        => WriteAsync(index, null, true);

    public Task WriteAsync(IndexEntity index, Action<IndexWriter> action)
        => WriteAsync(index, action, false);

    private Task WriteAsync(IndexEntity index, Action<IndexWriter> action, bool close)
    {
        if (!_writers.TryGetValue(index.IndexFullName, out var writer))
        {
            var metadata = index.As<LuceneIndexMetadata>();
            var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();

            var analyzer = _luceneAnalyzerManager.CreateAnalyzer(metadata.AnalyzerName ?? LuceneConstants.DefaultAnalyzer);

            lock (this)
            {
                if (!_writers.TryGetValue(index.IndexFullName, out writer))
                {
                    var directory = CreateDirectory(index.IndexFullName);
                    var config = new IndexWriterConfig(queryMetadata.DefaultVersion, analyzer)
                    {
                        OpenMode = OpenMode.CREATE_OR_APPEND,
                        WriteLockTimeout = LuceneLock.LOCK_POLL_INTERVAL * 3,
                    };

                    writer = new IndexWriterWrapper(directory, config);

                    if (close)
                    {
                        if (action is not null)
                        {
                            action.Invoke(writer);

                            if (_indexPools.TryRemove(index.IndexFullName, out var pool))
                            {
                                pool.MakeDirty();
                                pool.Release();
                            }
                        }

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

    private string GetFullPath(string indexFullName)
        => PathExtensions.Combine(_rootPath, indexFullName);

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

    ~LuceneIndexStore()
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
