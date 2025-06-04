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

public sealed class LuceneIndexStore : ILuceneIndexStore, IDisposable
{
    private readonly ConcurrentDictionary<string, IndexReaderPool> _indexPools = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IndexWriterWrapper> _writers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

    private static readonly object _synLock = new();
    private readonly object _lock = new();

    private readonly string _rootPath;
    private readonly IClock _clock;
    private readonly LuceneAnalyzerManager _analyzerManager;
    private readonly ILogger<LuceneIndexStore> _logger;

    private bool _disposed;

    public LuceneIndexStore(
        IClock clock,
        ShellSettings shellSettings,
        IOptions<ShellOptions> shellOptions,
        LuceneAnalyzerManager analyzerManager,
        ILogger<LuceneIndexStore> logger)
    {
        _clock = clock;
        _analyzerManager = analyzerManager;
        _logger = logger;
        _rootPath = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name, "Lucene");
        Directory.CreateDirectory(_rootPath);
    }

    public Task<bool> ExistsAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        try
        {
            var path = GetFullPath(indexFullName);

            return Task.FromResult(Directory.Exists(path));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of index: {IndexFullName}", indexFullName);

            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var removed = false;

        lock (_lock)
        {
            if (_writers.TryGetValue(indexFullName, out var writer))
            {
                writer.IsClosing = true;
                writer.Dispose();
                removed = _writers.TryRemove(indexFullName, out var removedWriter);
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
        }

        return Task.FromResult(removed);
    }

    public async Task SearchAsync(IndexEntity index, Func<IndexSearcher, Task> searcher)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(searcher);

        if (!await ExistsAsync(index.IndexFullName))
        {
            return;
        }

        using (var reader = GetReader(index.IndexFullName))
        {
            var indexSearcher = new IndexSearcher(reader.IndexReader);
            await searcher(indexSearcher);
        }

        _timestamps.AddOrUpdate(index.IndexFullName, _clock.UtcNow, (key, oldValue) => _clock.UtcNow);
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

            var analyzer = _analyzerManager.CreateAnalyzer(metadata.AnalyzerName ?? LuceneConstants.DefaultAnalyzer);

            lock (_lock)
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
                        action?.Invoke(writer);

                        if (_indexPools.TryRemove(index.IndexFullName, out var pool))
                        {
                            pool.MakeDirty();
                            pool.Release();
                        }

                        writer.IsClosing = true;
                        writer.Dispose();

                        return Task.CompletedTask;
                    }

                    _timestamps.AddOrUpdate(index.IndexFullName, _clock.UtcNow, (key, oldValue) => _clock.UtcNow);
                    _writers.AddOrUpdate(index.IndexFullName, writer, (key, oldValue) => writer);
                }
            }
        }

        if (writer.IsClosing)
        {
            _writers.TryRemove(index.IndexFullName, out var removedWriter);

            removedWriter?.Dispose();

            return Task.CompletedTask;
        }

        if (action is not null)
        {
            action.Invoke(writer);

            if (_indexPools.TryRemove(index.IndexFullName, out var pool))
            {
                pool.MakeDirty();
                pool.Release();
            }
        }

        _timestamps.AddOrUpdate(index.IndexFullName, _clock.UtcNow, (key, oldValue) => _clock.UtcNow);

        return Task.CompletedTask;
    }

    private IndexReaderPool.IndexReaderLease GetReader(string indexFullName)
    {
        var pool = _indexPools.GetOrAdd(indexFullName, n =>
        {
            var path = new DirectoryInfo(GetFullPath(indexFullName));

            var directory = FSDirectory.Open(path);

            var reader = DirectoryReader.Open(directory);

            return new IndexReaderPool(reader);
        });

        return pool.Acquire();
    }

    private string GetFullPath(string indexFullName)
        => PathExtensions.Combine(_rootPath, indexFullName);

    private FSDirectory CreateDirectory(string indexFullName)
    {
        lock (_lock)
        {
            var path = new DirectoryInfo(GetFullPath(indexFullName));

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
        lock (_lock)
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
    public IndexWriterWrapper(LDirectory directory, IndexWriterConfig config)
        : base(directory, config)
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
