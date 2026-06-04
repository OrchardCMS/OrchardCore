using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.VersionPruning.Models;
using OrchardCore.Contents.VersionPruning.Services;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Modules;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Serialization;
using YesSql.Sql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.Contents.VersionPruning;

public class ContentVersionPruningServiceTests : IAsyncLifetime
{
    private static readonly DateTime _now = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private IStore _store;
    private string _tempFilename;

    public async ValueTask InitializeAsync()
    {
        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(
            new Configuration().UseSqLite($"Data Source={_tempFilename};Cache=Shared"));

        var derivedOptions = new Mock<IOptions<JsonDerivedTypesOptions>>();
        derivedOptions.Setup(x => x.Value).Returns(new JsonDerivedTypesOptions());

        var jsonOptions = new DocumentJsonSerializerOptions();
        new DocumentJsonSerializerOptionsConfiguration(derivedOptions.Object).Configure(jsonOptions);

        var jsonSerializerOptions = new Mock<IOptions<DocumentJsonSerializerOptions>>();
        jsonSerializerOptions.Setup(x => x.Value).Returns(jsonOptions);

        _store.Configuration.ContentSerializer = new DefaultContentJsonSerializer(jsonSerializerOptions.Object);

        await using (var session = _store.CreateSession())
        {
            var builder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync());

            await builder.CreateMapIndexTableAsync<ContentItemIndex>(table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentItemVersionId", c => c.WithLength(26))
                .Column<bool>("Latest")
                .Column<bool>("Published")
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<DateTime>("ModifiedUtc", column => column.Nullable())
                .Column<DateTime>("PublishedUtc", column => column.Nullable())
                .Column<DateTime>("CreatedUtc", column => column.Nullable())
                .Column<string>("Owner", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
                .Column<string>("Author", column => column.Nullable().WithLength(ContentItemIndex.MaxAuthorSize))
                .Column<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
            );

            await session.SaveChangesAsync();
        }

        _store.RegisterIndexes<ContentItemIndexProvider>();
    }

    public ValueTask DisposeAsync()
    {
        _store?.Dispose();
        _store = null;

        if (_tempFilename != null && File.Exists(_tempFilename))
        {
            try
            {
                File.Delete(_tempFilename);
            }
            catch
            {
            }
        }

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task PruneVersionsAsync_KeepsNewestAndVersionsWithinRetention()
    {
        await SaveVersionsAsync(
            Version("item-1", "v1", "Blog", _now.AddDays(-90)),
            Version("item-1", "v2", "Blog", _now.AddDays(-10)),
            Version("item-1", "v3", "Blog", _now.AddDays(-2)));

        var settings = new ContentVersionPruningSettings
        {
            RetentionDays = 30,
            VersionsToKeep = 1,
            ContentTypes = ["Blog"],
        };

        var deleted = await RunPruningAsync(settings);

        Assert.Equal(1, deleted);

        var remaining = await GetRemainingVersionIdsAsync();
        Assert.Equal(["v2", "v3"], remaining.OrderBy(x => x));
    }

    [Fact]
    public async Task PruneVersionsAsync_OnlyPrunesSelectedContentTypes()
    {
        await SaveVersionsAsync(
            Version("blog-1", "b1", "Blog", _now.AddDays(-90)),
            Version("blog-1", "b2", "Blog", _now.AddDays(-80)),
            Version("page-1", "p1", "Page", _now.AddDays(-90)),
            Version("page-1", "p2", "Page", _now.AddDays(-80)));

        var settings = new ContentVersionPruningSettings
        {
            RetentionDays = 30,
            VersionsToKeep = 0,
            ContentTypes = ["Blog"],
        };

        var deleted = await RunPruningAsync(settings);

        Assert.Equal(2, deleted);

        var remaining = await GetRemainingVersionIdsAsync();
        Assert.Equal(["p1", "p2"], remaining.OrderBy(x => x));
    }

    [Fact]
    public async Task PruneVersionsAsync_NeverDeletesLatestOrPublished()
    {
        await SaveVersionsAsync(
            new ContentItem { ContentItemId = "item-1", ContentItemVersionId = "v-latest", ContentType = "Blog", ModifiedUtc = _now.AddDays(-90), Latest = true },
            new ContentItem { ContentItemId = "item-1", ContentItemVersionId = "v-published", ContentType = "Blog", ModifiedUtc = _now.AddDays(-90), Published = true },
            Version("item-1", "v-old", "Blog", _now.AddDays(-90)));

        var settings = new ContentVersionPruningSettings
        {
            RetentionDays = 30,
            VersionsToKeep = 0,
            ContentTypes = ["Blog"],
        };

        var deleted = await RunPruningAsync(settings);

        Assert.Equal(1, deleted);

        var remaining = await GetRemainingVersionIdsAsync();
        Assert.Equal(["v-latest", "v-published"], remaining.OrderBy(x => x));
    }

    [Fact]
    public async Task PruneVersionsAsync_EmptyContentTypes_DeletesNothing()
    {
        await SaveVersionsAsync(
            Version("item-1", "v1", "Blog", _now.AddDays(-90)),
            Version("item-1", "v2", "Blog", _now.AddDays(-80)));

        var settings = new ContentVersionPruningSettings
        {
            RetentionDays = 30,
            VersionsToKeep = 0,
            ContentTypes = [],
        };

        var deleted = await RunPruningAsync(settings);

        Assert.Equal(0, deleted);
        Assert.Equal(2, (await GetRemainingVersionIdsAsync()).Count);
    }

    [Fact]
    public async Task PruneVersionsAsync_ProcessesMoreThanOneBatch()
    {
        // Exercises the batching/flush path with more candidates than the internal batch size.
        const int versionCount = 250;
        var versions = new ContentItem[versionCount];
        for (var i = 0; i < versionCount; i++)
        {
            versions[i] = Version("item-1", $"v{i:D4}", "Blog", _now.AddDays(-100 - i));
        }

        await SaveVersionsAsync(versions);

        var settings = new ContentVersionPruningSettings
        {
            RetentionDays = 30,
            VersionsToKeep = 0,
            ContentTypes = ["Blog"],
        };

        var deleted = await RunPruningAsync(settings);

        Assert.Equal(versionCount, deleted);
        Assert.Empty(await GetRemainingVersionIdsAsync());
    }

    private async Task<int> RunPruningAsync(ContentVersionPruningSettings settings)
    {
        await using var session = _store.CreateSession();

        var clock = new Mock<IClock>();
        clock.SetupGet(x => x.UtcNow).Returns(_now);

        var service = new ContentVersionPruningService(session, clock.Object, NullLogger<ContentVersionPruningService>.Instance);

        var deleted = await service.PruneVersionsAsync(settings);

        await session.SaveChangesAsync();

        return deleted;
    }

    private async Task SaveVersionsAsync(params ContentItem[] versions)
    {
        await using var session = _store.CreateSession();

        foreach (var version in versions)
        {
            await session.SaveAsync(version);
        }

        await session.SaveChangesAsync();
    }

    private async Task<List<string>> GetRemainingVersionIdsAsync()
    {
        await using var session = _store.CreateSession();

        var indexes = await session.QueryIndex<ContentItemIndex>().ListAsync();

        return indexes.Select(x => x.ContentItemVersionId).ToList();
    }

    private static ContentItem Version(string contentItemId, string versionId, string contentType, DateTime? modifiedUtc)
        => new()
        {
            ContentItemId = contentItemId,
            ContentItemVersionId = versionId,
            ContentType = contentType,
            DisplayText = $"{contentItemId} - {versionId}",
            ModifiedUtc = modifiedUtc,
            Latest = false,
            Published = false,
        };
}
