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

        Assert.Equal(7, version);
        Assert.Equal(["ContentItemId", "DocumentId", "Published", "Latest"], await GetIndexColumnsAsync());
    }

    [Fact]
    public async Task UpdateFrom6Async_CreatesIndexLedByContentItemId()
    {
        // The table and indexes as a tenant at version 6 has them.
        await RunAsync(migrations => migrations.CreateAsync());
        await using (var connection = _store.Configuration.ConnectionFactory.CreateConnection())
        {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"DROP INDEX {await GetIndexNameAsync()}";
            await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(await GetIndexColumnsAsync());

        var version = await RunAsync(migrations => migrations.UpdateFrom6Async());

        Assert.Equal(7, version);
        Assert.Equal("ContentItemId", (await GetIndexColumnsAsync()).FirstOrDefault());
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

    private async Task<string> GetIndexNameAsync()
    {
        await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND name LIKE '%IDX_ContentItemIndex_ContentItemId'";

        return (string)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
    }

    private async Task<List<string>> GetIndexColumnsAsync()
    {
        var name = await GetIndexNameAsync();
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
