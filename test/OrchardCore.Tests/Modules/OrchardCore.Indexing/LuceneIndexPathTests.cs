using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene.Core;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class LuceneIndexPathTests
{
    [Fact]
    public async Task Remove_UnsafeStoredIndexPath_CannotDeleteSiblingDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "pomi-lucene-path-" + Guid.NewGuid().ToString("N"));
        try
        {
            var sibling = Path.Combine(root, "Sites", "Tenant", "outside");
            Directory.CreateDirectory(sibling);
            var marker = Path.Combine(sibling, "keep.txt");
            await File.WriteAllTextAsync(marker, "keep", TestContext.Current.CancellationToken);
            using var store = new LuceneIndexStore(Mock.Of<IClock>(), new ShellSettings { Name = "Tenant" },
                Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
                new LuceneAnalyzerManager(Options.Create(new LuceneOptions())), NullLogger<LuceneIndexStore>.Instance);
            var profile = new IndexProfile { Id = "id", IndexFullName = "../outside" };

            await Assert.ThrowsAsync<ArgumentException>(() => store.RemoveAsync(profile));

            Assert.Equal("keep", await File.ReadAllTextAsync(marker, TestContext.Current.CancellationToken));
            Assert.False(await store.ExistsAsync("../outside"));
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }
}
