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
    private const string ContentTypeIndex = "IDX_ContentItemIndex_ContentType";
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
    public async Task UpdateFrom7Async_CreatesSameIndexesAsCreateAsync()
    {
        // The table and indexes as a tenant at version 7 has them.
        await RunAsync(migrations => migrations.CreateAsync());
        var contentTypeColumns = await GetIndexColumnsAsync(ContentTypeIndex);
        var ownerColumns = await GetIndexColumnsAsync(OwnerIndex);
        await DropIndexAsync(ContentTypeIndex);
        await DropIndexAsync(OwnerIndex);

        Assert.Empty(await GetIndexColumnsAsync(ContentTypeIndex));
        Assert.Empty(await GetIndexColumnsAsync(OwnerIndex));

        var version = await RunAsync(migrations => migrations.UpdateFrom7Async());

        Assert.Equal(8, version);
        Assert.Equal(contentTypeColumns, await GetIndexColumnsAsync(ContentTypeIndex));
        Assert.Equal(ownerColumns, await GetIndexColumnsAsync(OwnerIndex));
    }

    private async Task<int> RunAsync(Func<ContentMigrations, Task<int>> step)
    {
        await using var session = _store.CreateSession();
        var migrations = new ContentMigrations(Mock.Of<IContentDefinitionManager>())
        {
            SchemaBuilder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync()),
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
