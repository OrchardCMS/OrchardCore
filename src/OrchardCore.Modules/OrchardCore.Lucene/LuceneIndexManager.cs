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
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;
using Directory = System.IO.Directory;
using LDirectory = Lucene.Net.Store.Directory;

namespace OrchardCore.Lucene
{
    /// <summary>
    /// Provides methods to manage physical Lucene indices.
    /// This class is provided as a singleton to that the index searcher can be reused across requests.
    /// </summary>
    public class LuceneIndexManager : IDisposable
    {
        private readonly IClock _clock;
        private readonly ILogger<LuceneIndexManager> _logger;
        private readonly string _rootPath;
        private readonly DirectoryInfo _rootDirectory;
        private bool _disposing;
        private ConcurrentDictionary<string, Lazy<IndexReaderPool>> _indexPools = new ConcurrentDictionary<string, Lazy<IndexReaderPool>>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, Lazy<IndexWriter>> _writers = new ConcurrentDictionary<string, Lazy<IndexWriter>>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, DateTime> _timestamps = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private static object _synLock = new object();

        public LuceneIndexManager(
            IClock clock,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ILogger<LuceneIndexManager> logger,
            LuceneAnalyzerManager luceneAnalyzerManager
            )
        {
            _clock = clock;
            _logger = logger;
            _rootPath = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name, "Lucene");

            _rootDirectory = Directory.CreateDirectory(_rootPath);
            _luceneAnalyzerManager = luceneAnalyzerManager;
        }

        public void CreateIndex(string indexName)
        {
            lock (this)
            {
                Write(indexName);

                if (_writers.TryRemove(indexName, out var writer))
                {
                    writer.Value.Dispose();
                }
            }
        }

        public void DeleteDocuments(string indexName, IEnumerable<string> contentItemIds)
        {
            Write(indexName, writer =>
            {
                writer.DeleteDocuments(contentItemIds.Select(x => new Term("ContentItemId", x)).ToArray());

                writer.Commit();

                if (_indexPools.TryRemove(indexName, out var pool))
                {
                    pool.Value.MakeDirty();
                    pool.Value.Release();
                }
            });
        }

        /// <summary>
        /// Deletes an index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        public void DeleteIndex(string indexName)
        {
            lock (this)
            {
                // Close the writer for this index if it exists
                if (_writers.TryGetValue(indexName, out var writer))
                {
                    writer.Value.Dispose();
                }

                if (_indexPools.TryRemove(indexName, out var reader))
                {
                    reader.Value.Dispose();
                }

                _timestamps.TryRemove(indexName, out var timestamp);
                
                var indexFolder = PathExtensions.Combine(_rootPath, indexName);

                if (Directory.Exists(indexFolder))
                {
                    try
                    {
                        Directory.Delete(indexFolder, true);
                    }
                    catch { }
                }

                _writers.TryRemove(indexName, out writer);
            }
        }

        public bool Exists(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }

            return Directory.Exists(PathExtensions.Combine(_rootPath, indexName));
        }

        public IEnumerable<string> List()
        {
            return _rootDirectory
                .GetDirectories()
                .Select(x => x.Name);
        }

        public void StoreDocuments(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            Write(indexName, writer =>
            {
                foreach (var indexDocument in indexDocuments)
                {
                    writer.AddDocument(CreateLuceneDocument(indexDocument));
                }

                writer.Commit();

                if (_indexPools.TryRemove(indexName, out var pool))
                {
                    pool.Value.MakeDirty();
                    pool.Value.Release();
                }
            });
        }

        public async Task SearchAsync(string indexName, Func<IndexSearcher, Task> searcher)
        {
            using (var reader = GetReader(indexName))
            {
                var indexSearcher = new IndexSearcher(reader.IndexReader);
                await searcher(indexSearcher);
            }

            _timestamps[indexName] = _clock.UtcNow;
        }

        public void Read(string indexName, Action<IndexReader> reader)
        {
            using (var indexReader = GetReader(indexName))
            {
                reader(indexReader.IndexReader);
            }

            _timestamps[indexName] = _clock.UtcNow;
        }

        /// <summary>
        /// Returns a list of open indices and the last time they were accessed.
        /// </summary>
        public IReadOnlyDictionary<string, DateTime> GetTimestamps()
        {
            return new ReadOnlyDictionary<string, DateTime>(_timestamps);
        }

        private Document CreateLuceneDocument(DocumentIndex documentIndex)
        {
            var doc = new Document
            {
                // Always store the content item id
                new StringField("ContentItemId", documentIndex.ContentItemId.ToString(), Field.Store.YES)
            };

            foreach (var entry in documentIndex.Entries)
            {
                var store = entry.Options.HasFlag(DocumentIndexOptions.Store)
                            ? Field.Store.YES
                            : Field.Store.NO;

                if (entry.Value == null)
                {
                    continue;
                }

                switch (entry.Type)
                {
                    case DocumentIndex.Types.Boolean:
                        // store "true"/"false" for booleans
                        doc.Add(new StringField(entry.Name, Convert.ToString(entry.Value).ToLowerInvariant(), store));
                        break;

                    case DocumentIndex.Types.DateTime:
                        if (entry.Value is DateTimeOffset)
                        {
                            doc.Add(new StringField(entry.Name, DateTools.DateToString(((DateTimeOffset)entry.Value).UtcDateTime, DateTools.Resolution.SECOND), store));
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Name, DateTools.DateToString(((DateTime)entry.Value).ToUniversalTime(), DateTools.Resolution.SECOND), store));
                        }
                        break;

                    case DocumentIndex.Types.Integer:
                        doc.Add(new Int32Field(entry.Name, Convert.ToInt32(entry.Value), store));
                        break;

                    case DocumentIndex.Types.Number:
                        doc.Add(new DoubleField(entry.Name, Convert.ToDouble(entry.Value), store));
                        break;

                    case DocumentIndex.Types.Text:
                        if (entry.Options.HasFlag(DocumentIndexOptions.Analyze))
                        {
                            doc.Add(new TextField(entry.Name, Convert.ToString(entry.Value), store));
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Name, Convert.ToString(entry.Value), store));
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

                // Lucene is not thread safe on this call
                lock (_synLock)
                {
                    return FSDirectory.Open(path);
                }
            }
        }

        private void Write(string indexName, Action<IndexWriter> action = null)
        {
            var writer = _writers.GetOrAdd(indexName, name =>
            {
                return new Lazy<IndexWriter>(() =>
                {
                    var directory = CreateDirectory(name);
                    var analyzer = _luceneAnalyzerManager.CreateAnalyzer(LuceneSettings.StandardAnalyzer);
                    var config = new IndexWriterConfig(LuceneSettings.DefaultVersion, analyzer)
                    {
                        OpenMode = OpenMode.CREATE_OR_APPEND
                    };

                    return new IndexWriter(directory, config);
                });
            }).Value;

            action?.Invoke(writer);

            _timestamps[indexName] = _clock.UtcNow;
        }

        private IndexReaderPool.IndexReaderLease GetReader(string indexName)
        {
            var pool = _indexPools.GetOrAdd(indexName, n =>
            {
                return new Lazy<IndexReaderPool>(() =>
                {
                    var directory = CreateDirectory(indexName);
                    var reader = DirectoryReader.Open(directory);
                    return new IndexReaderPool(reader);
                });
            }).Value;

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
                        _logger.LogInformation("Freeing writer for: " + writer.Key);
                    }

                    writer.Value.Value.Dispose();
                }

                foreach (var reader in _indexPools)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Freeing reader for: " + reader.Key);
                    }

                    reader.Value.Value.Dispose();
                }

                _writers.Clear();
                _indexPools.Clear();
                _timestamps.Clear();
            }
        }

        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;

            FreeReaderWriter();
        }

        ~LuceneIndexManager()
        {
            Dispose();
        }
    }

    internal class IndexReaderPool : IDisposable
    {
        private bool _dirty;
        private int _count;
        private IndexReader _reader;

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

        public struct IndexReaderLease : IDisposable
        {
            private IndexReaderPool _pool;

            public IndexReaderLease(IndexReaderPool pool, IndexReader reader)
            {
                _pool = pool;
                Interlocked.Increment(ref _pool._count);
                IndexReader = reader;
            }

            public IndexReader IndexReader { get; }

            public void Dispose()
            {
                var count = Interlocked.Decrement(ref _pool._count);
                _pool.Release();
            }
        }
    }
}
