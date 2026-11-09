using OrchardCore.Workflows.Models;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Sql;
using WorkflowMigrations = OrchardCore.Workflows.Migrations;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows;

public class WorkflowMigrationsTests : IAsyncLifetime
{
    private IStore _store;
    private string _tempFilename;

    public async ValueTask InitializeAsync()
    {
        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(
            new Configuration().UseSqLite($"Data Source={_tempFilename};Cache=Shared"));
        await _store.InitializeCollectionAsync(WorkflowExecutionRecord.Collection);
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
    public async Task UpdateFrom4To8_IndexesInUpdatesDisabled_IndexesOnlyNewTables()
    {
        // The tables as a tenant at version 4 has them.
        var version = await RunAsync(migrations => migrations.CreateAsync());

        foreach (var step in new Func<WorkflowMigrations, Task<int>>[]
        {
            migrations => migrations.UpdateFrom4Async(),
            migrations => migrations.UpdateFrom5Async(),
            migrations => migrations.UpdateFrom6Async(),
            migrations => migrations.UpdateFrom7Async(),
            migrations => migrations.UpdateFrom8Async(),
        })
        {
            version = await RunAsync(step, createIndexesInUpdates: false);
        }

        Assert.Equal(9, version);

        // The tables created by the update steps are indexed, the ones that existed before aren't.
        Assert.Equal(["IDX_FK_WorkflowTypeDraftIndex", "IDX_WorkflowTypeDraftIndex_DocumentId"], await GetIndexesAsync("WorkflowTypeDraftIndex"));
        Assert.Equal(["IDX_FK_WorkflowTypeVersionIndex", "IDX_WorkflowTypeVersionIndex_DocumentId"], await GetIndexesAsync("WorkflowTypeVersionIndex"));
        Assert.Equal(
            ["IDX_FK_WorkflowJournal_WorkflowExecutionRecordIndex", "IDX_WorkflowExecutionRecordIndex_DocumentId"],
            await GetIndexesAsync("WorkflowJournal_WorkflowExecutionRecordIndex"));
        Assert.Equal(["IDX_FK_WorkflowIndex", "IDX_WorkflowIndex_DocumentId"], await GetIndexesAsync("WorkflowIndex"));
        Assert.Equal(
            ["IDX_FK_WorkflowBlockingActivitiesIndex", "IDX_WFBlockingActivities_DocumentId_ActivityId", "IDX_WFBlockingActivities_DocumentId_ActivityName"],
            await GetIndexesAsync("WorkflowBlockingActivitiesIndex"));
    }

    private async Task<int> RunAsync(Func<WorkflowMigrations, Task<int>> step, bool createIndexesInUpdates = true)
    {
        await using var session = _store.CreateSession();
        var migrations = new WorkflowMigrations
        {
            SchemaBuilder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync()),
            CreateIndexesInUpdates = createIndexesInUpdates,
        };

        var version = await step(migrations);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        return version;
    }

    private async Task<List<string>> GetIndexesAsync(string table)
    {
        await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = '{table}' AND name NOT LIKE 'sqlite_%' ORDER BY name";

        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }
}
