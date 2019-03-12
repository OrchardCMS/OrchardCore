using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Lucene;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;
using Xunit;

namespace OrchardCore.Tests.Lucene
{
    public class LuceneIndexManagerTests
    {
        public LuceneIndexManager CreateLuceneManager(string name)
        {
            var clock = new Clock();

            var shellOptions = new Mock<IOptions<ShellOptions>>();
            shellOptions.Setup(c => c.Value).Returns(new ShellOptions
            {
                ShellsApplicationDataPath = "App_Data",
                ShellsContainerName = "Sites"
            });

            var shellSettings = new ShellSettings
            {
                Name = name
            };

            var luceneOptions = new Mock<IOptions<LuceneOptions>>();
            luceneOptions.Setup(c => c.Value).Returns(new LuceneOptions());

            var luceneAnalyzerManager = new LuceneAnalyzerManager(luceneOptions.Object);

            var rootPath = PathExtensions.Combine("App_Data", "Sites", name, "Lucene");

            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, true);
            }

            return new LuceneIndexManager(clock, shellOptions.Object, shellSettings, new NullLogger<LuceneIndexManager>(), luceneAnalyzerManager);
        }

        [Fact]
        public async Task DocumentsCanBeIndexedAndSearched()
        {
            var _luceneIndexManager = CreateLuceneManager("DocumentsCanBeIndexedAndSearched");
            var indexName = "search";

            var documentIndex = new DocumentIndex("abc");
            documentIndex.Set("title", "man walked on the moon", DocumentIndexOptions.Store);

            _luceneIndexManager.StoreDocuments(indexName, new[] { documentIndex });

            await _luceneIndexManager.SearchAsync(indexName, s =>
            {
                var doc = s.Doc(0);
                Assert.NotNull(doc);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DocumentsCanBeCreatedConcurrently()
        {
            var _luceneIndexManager = CreateLuceneManager("DocumentsCanBeCreatedConcurrently");
            var indexName = "search";

            var concurrency = 20;
            var MaxTransactions = 10000;

            var counter = 0;
            var stopping = false;

            var tasks = Enumerable.Range(1, concurrency).Select(i => Task.Run(() =>
            {
                while (!stopping && Interlocked.Add(ref counter, 1) < MaxTransactions)
                {
                    var documentIndex = new DocumentIndex("abc");
                    documentIndex.Set("title", "man walked on the moon", DocumentIndexOptions.Store);

                    _luceneIndexManager.StoreDocuments(indexName, new[] { documentIndex });
                }
            })).ToList();

            // Running for a maximum of 5 seconds
            tasks.Add(Task.Delay(TimeSpan.FromSeconds(5)));

            await Task.WhenAny(tasks);

            // Flushing tasks
            stopping = true;
            await Task.WhenAll(tasks);

            await _luceneIndexManager.SearchAsync(indexName, s =>
            {
                var doc = s.Doc(0);
                Assert.NotNull(doc);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DocumentsCanBeDeletedAndCreatedConcurrently()
        {
            var _luceneIndexManager = CreateLuceneManager("DocumentsCanBeDeletedAndCreatedConcurrently");
            var indexName = "search";

            var concurrency = 20;
            var MaxTransactions = 10000;

            var counter = 0;
            var stopping = false;

            var tasks = Enumerable.Range(1, concurrency).Select(i => Task.Run(() =>
            {
                while (!stopping && Interlocked.Add(ref counter, 1) < MaxTransactions)
                {
                    var id = i.ToString();

                    var documentIndex = new DocumentIndex(id);
                    documentIndex.Set("title", "man walked on the moon", DocumentIndexOptions.Store);

                    _luceneIndexManager.DeleteDocuments(indexName, new[] { id });
                    _luceneIndexManager.StoreDocuments(indexName, new[] { documentIndex });
                }
            })).ToList();

            // Running for a maximum of 5 seconds
            tasks.Add(Task.Delay(TimeSpan.FromSeconds(5)));

            await Task.WhenAny(tasks);

            // Flushing tasks
            stopping = true;
            await Task.WhenAll(tasks);

            await _luceneIndexManager.SearchAsync(indexName, s =>
            {
                var doc = s.Doc(0);
                Assert.NotNull(doc);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DocumentsCanBeDeletedCreatedAndSearchedConcurrently()
        {
            var _luceneIndexManager = CreateLuceneManager("DocumentsCanBeDeletedCreatedAndSearchedConcurrently");
            var indexName = "search";

            var concurrency = 20;
            var MaxTransactions = 10000;

            var counter = 0;
            var stopping = false;

            var tasks = Enumerable.Range(1, concurrency).Select(i => Task.Run(async () =>
            {
                while (!stopping && Interlocked.Add(ref counter, 1) < MaxTransactions)
                {
                    var id = i.ToString();

                    var documentIndex = new DocumentIndex(id);
                    documentIndex.Set("title", "man walked on the moon", DocumentIndexOptions.Store);

                    _luceneIndexManager.DeleteDocuments(indexName, new[] { id });
                    _luceneIndexManager.StoreDocuments(indexName, new[] { documentIndex });

                    await _luceneIndexManager.SearchAsync(indexName, s =>
                    {
                        var doc = s.Doc(0);
                        Assert.NotNull(doc);

                        return Task.CompletedTask;
                    });
                }
            })).ToList();

            // Running for a maximum of 5 seconds
            tasks.Add(Task.Delay(TimeSpan.FromSeconds(5)));

            await Task.WhenAny(tasks);

            // Flushing tasks
            stopping = true;
            await Task.WhenAll(tasks);

            await _luceneIndexManager.SearchAsync(indexName, s =>
            {
                var doc = s.Doc(0);
                Assert.NotNull(doc);

                return Task.CompletedTask;
            });
        }
    }
}
