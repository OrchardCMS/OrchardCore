using Lucene.Net.Analysis.Standard;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene;
using OrchardCore.Lucene.Core;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.Lucene;

public sealed class LuceneIndexStoreTests
{
    [Fact]
    public async Task SearchAsync_AfterCreatingEmptyIndex_OpensEmptyReader()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "oc-lucene-index-store-tests", Guid.NewGuid().ToString("N"));

        try
        {
            var clock = new Mock<IClock>();
            clock.SetupGet(clock => clock.UtcNow).Returns(DateTime.UtcNow);

            var luceneOptions = new LuceneOptions();
            luceneOptions.Analyzers.Add(new LuceneAnalyzer(
                LuceneConstants.DefaultAnalyzer,
                new StandardAnalyzer(LuceneConstants.DefaultVersion)));

            using var store = new LuceneIndexStore(
                clock.Object,
                new ShellSettings { Name = "Default" },
                Options.Create(new ShellOptions
                {
                    ShellsApplicationDataPath = rootPath,
                    ShellsContainerName = "Sites",
                }),
                new LuceneAnalyzerManager(Options.Create(luceneOptions)),
                NullLogger<LuceneIndexStore>.Instance);

            var index = new IndexProfile
            {
                Id = "profile",
                IndexFullName = "index",
            };

            await store.WriteAndClose(index);

            var searchCompleted = false;

            await store.SearchAsync(index, searcher =>
            {
                searchCompleted = true;
                Assert.Equal(0, searcher.IndexReader.NumDocs);
                return Task.CompletedTask;
            });

            Assert.True(searchCompleted);
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }
}
