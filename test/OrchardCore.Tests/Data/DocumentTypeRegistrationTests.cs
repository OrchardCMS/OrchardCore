using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using YesSql;
using YesSql.Provider.Sqlite;

namespace OrchardCore.Tests.Data;

public class DocumentTypeRegistrationTests
{
    [Fact]
    public void AddDocumentTypeRegistersTheTypeInTheOptions()
    {
        var services = new ServiceCollection();

        services.AddDocumentType<SampleDocumentA>();

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<DocumentTypeOptions>>().Value;

        Assert.Contains(typeof(SampleDocumentA), options.Types);
    }

    [Fact]
    public async Task ReadingByIdThrowsWhenTheRowTypeIsNotRegistered()
    {
        var tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var id = await SaveSampleDocumentAAsync(tempFilename);

            // A brand-new store has an empty type cache, like after an application restart.
            var store = await CreateStoreAsync(tempFilename);

            await using var session = store.CreateSession();

            // A by-id read has no type filter, so it loads the 'SampleDocumentA' row even though
            // 'SampleDocumentB' was requested. The row type was never registered in this process.
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => session.GetAsync<SampleDocumentB>([id], cancellationToken: TestContext.Current.CancellationToken));

            store.Dispose();
        }
        finally
        {
            DeleteTempFile(tempFilename);
        }
    }

    [Fact]
    public async Task ReadingByIdDoesNotThrowWhenTheRowTypeIsPreRegistered()
    {
        var tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var id = await SaveSampleDocumentAAsync(tempFilename);

            var store = await CreateStoreAsync(tempFilename);

            // Pre-register the document type, as done at store creation from the registered
            // 'DocumentTypeOptions', so the reverse lookup of the persisted type name succeeds.
            _ = store.TypeNames[typeof(SampleDocumentA)];

            await using var session = store.CreateSession();

            var result = await session.GetAsync<SampleDocumentB>([id], cancellationToken: TestContext.Current.CancellationToken);

            // The row is a 'SampleDocumentA' which is not assignable to 'SampleDocumentB', so it is
            // simply filtered out instead of crashing the read.
            Assert.Empty(result);

            store.Dispose();
        }
        finally
        {
            DeleteTempFile(tempFilename);
        }
    }

    private static async Task<long> SaveSampleDocumentAAsync(string tempFilename)
    {
        var store = await CreateStoreAsync(tempFilename);

        long id;
        await using (var session = store.CreateSession())
        {
            var document = new SampleDocumentA { Name = "sample" };
            await session.SaveAsync(document);
            await session.SaveChangesAsync();
            id = document.Id;
        }

        store.Dispose();

        return id;
    }

    private static Task<IStore> CreateStoreAsync(string tempFilename)
        => StoreFactory.CreateAndInitializeAsync(
            new Configuration().UseSqLite($"Data Source={tempFilename};Cache=Shared"));

    private static void DeleteTempFile(string tempFilename)
    {
        if (File.Exists(tempFilename))
        {
            try
            {
                File.Delete(tempFilename);
            }
            catch (IOException)
            {
            }
        }
    }

    public sealed class SampleDocumentA : IDocument
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Identifier { get; set; }

        public bool IsReadOnly { get; set; }
    }

    public sealed class SampleDocumentB : IDocument
    {
        public long Id { get; set; }

        public string Identifier { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
