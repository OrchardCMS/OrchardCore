using Moq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Sql;
using ContentMigrations = OrchardCore.ContentManagement.Records.Migrations;

namespace OrchardCore.Tests.ContentManagement;

public class ContentItemIndexMigrationsTests : IAsyncLifetime
{
    private const string ContentItemIdIndex = "IDX_ContentItemIndex_ContentItemId";
    private const string ContentItemVersionIdIndex = "IDX_ContentItemIndex_ContentItemVersionId";
    private const string ContentTypeIndex = "IDX_ContentItemIndex_ContentType";
    private const string DocumentIdContentTypeIndex = "IDX_ContentItemIndex_DocumentId_ContentType";
    private const string ForeignKeyIndex = "IDX_FK_ContentItemIndex";
    private const string OwnerIndex = "IDX_ContentItemIndex_Owner";

    private IStore _store;
    private string _tempFilename;

    public async ValueTask InitializeAsync()
    {
        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(
            new Configuration().UseSqLite($"Data Source={_tempFilename};Cache=Shared"));
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
    public async Task CreateAsync_CreatesIndexLedByContentItemId()
    {
        var version = await RunAsync(migrations => migrations.CreateAsync());

        Assert.Equal(8, version);
        Assert.Equal(["ContentItemId", "DocumentId", "Published", "Latest"], await GetIndexColumnsAsync(ContentItemIdIndex));
    }

    [Fact]
    public async Task CreateAsync_CreatesIndexesLedByFilters()
    {
        await RunAsync(migrations => migrations.CreateAsync());

        Assert.Equal(["ContentType", "DocumentId", "Published", "Latest"], await GetIndexColumnsAsync(ContentTypeIndex));
        Assert.Equal(["Owner", "DocumentId", "Published", "Latest"], await GetIndexColumnsAsync(OwnerIndex));
        Assert.Equal(["ContentItemVersionId", "DocumentId"], await GetIndexColumnsAsync(ContentItemVersionIdIndex));
    }

    [Fact]
    public async Task CreateAsync_CreatesOnlyExpectedIndexes()
    {
        await RunAsync(migrations => migrations.CreateAsync());

        var indexes = await GetIndexesAsync();

        Assert.Equal(
            [ContentItemIdIndex, ContentItemVersionIdIndex, ContentTypeIndex, DocumentIdContentTypeIndex, OwnerIndex, ForeignKeyIndex],
            indexes.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task UpdateFrom6Async_CreatesIndexLedByContentItemId()
    {
        // The table and indexes as a tenant at version 6 has them.
        await RunAsync(migrations => migrations.CreateAsync());
        await DropIndexAsync(ContentItemIdIndex);

        Assert.Empty(await GetIndexColumnsAsync(ContentItemIdIndex));

        var version = await RunAsync(migrations => migrations.UpdateFrom6Async());

        Assert.Equal(7, version);
        Assert.Equal("ContentItemId", (await GetIndexColumnsAsync(ContentItemIdIndex)).FirstOrDefault());
    }

    [Fact]
    public async Task UpdateFrom7Async_EndsWithSameIndexesAsCreateAsync()
    {
        await RunAsync(migrations => migrations.CreateAsync());
        var expected = await GetIndexesAsync();

        // The indexes as a tenant at version 7 has them.
        foreach (var index in expected.Keys.Where(x => x != ForeignKeyIndex))
        {
            await DropIndexAsync(index);
        }

        await RunAsync(migrations => migrations.UpdateFrom5Async());
        await RunAsync(migrations => migrations.UpdateFrom6Async());

        Assert.Contains("IDX_ContentItemIndex_DocumentId_DisplayText", (await GetIndexesAsync()).Keys);

        var version = await RunAsync(migrations => migrations.UpdateFrom7Async());

        Assert.Equal(8, version);
        Assert.Equal(expected, await GetIndexesAsync());
    }

    [Fact]
    public async Task UpdateFrom6Async_IndexesInUpdatesDisabled_DoesNotCreateIndex()
    {
        await RunAsync(migrations => migrations.CreateAsync());
        await DropIndexAsync(ContentItemIdIndex);

        var version = await RunAsync(migrations => migrations.UpdateFrom6Async(), createIndexesInUpdates: false);

        Assert.Equal(7, version);
        Assert.Empty(await GetIndexColumnsAsync(ContentItemIdIndex));
    }

    [Fact]
    public async Task UpdateFrom7Async_IndexesInUpdatesDisabled_DropsIndexesWithoutCreatingOthers()
    {
        await RunAsync(migrations => migrations.CreateAsync());

        // The indexes as a tenant at version 7 has them, without the index of step 6.
        foreach (var index in (await GetIndexesAsync()).Keys.Where(x => x != ForeignKeyIndex))
        {
            await DropIndexAsync(index);
        }

        await RunAsync(migrations => migrations.UpdateFrom5Async());

        var version = await RunAsync(migrations => migrations.UpdateFrom7Async(), createIndexesInUpdates: false);

        Assert.Equal(8, version);
        Assert.Equal(
            [DocumentIdContentTypeIndex, ForeignKeyIndex],
            (await GetIndexesAsync()).Keys.Order(StringComparer.Ordinal));
    }

    private async Task<int> RunAsync(Func<ContentMigrations, Task<int>> step, bool createIndexesInUpdates = true)
    {
        await using var session = _store.CreateSession();
        var migrations = new ContentMigrations(Mock.Of<IContentDefinitionManager>())
        {
            SchemaBuilder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync()),
            CreateIndexesInUpdates = createIndexesInUpdates,
        };

        var version = await step(migrations);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        return version;
    }

    private async Task DropIndexAsync(string index)
    {
        await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP INDEX {await GetIndexNameAsync(index)}";
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }

    private async Task<string> GetIndexNameAsync(string index)
    {
        await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type = 'index' AND name LIKE '%{index}'";

        return (string)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
    }

    private async Task<Dictionary<string, List<string>>> GetIndexesAsync()
    {
        await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'ContentItemIndex' AND name NOT LIKE 'sqlite_%'";

        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            names.Add(reader.GetString(0));
        }

        var indexes = new Dictionary<string, List<string>>();
        foreach (var name in names)
        {
            indexes[name] = await GetIndexColumnsAsync(name);
        }

        return indexes;
    }

    private async Task<List<string>> GetIndexColumnsAsync(string index)
    {
        var name = await GetIndexNameAsync(index);
        if (name is null)
        {
            return [];
        }

        await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM pragma_index_info('{name}') ORDER BY seqno";

        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }
}
