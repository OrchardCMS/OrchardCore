using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

public sealed class WorkflowTypeStoreCacheTests : IAsyncLifetime
{
    private VersioningTestDatabase _database;

    public async ValueTask InitializeAsync()
        => _database = await VersioningTestDatabase.CreateAsync();

    public ValueTask DisposeAsync()
        => _database.DisposeAsync();

    [Fact]
    public async Task GetAsync_SameWorkflowTypeIdTwice_ReturnsTheSameInstanceFromTheScope()
    {
        var (session, _, types) = _database.CreateStores();

        await using (session)
        {
            await types.SaveAsync(NewType("type-1"));
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var (reading, _, store) = _database.CreateStores();

        await using (reading)
        {
            var first = await store.GetAsync("type-1");
            var second = await store.GetAsync("type-1");

            Assert.NotNull(first);
            Assert.Same(first, second);
        }
    }

    [Fact]
    public async Task ListAsync_AfterASave_IncludesTheSavedWorkflowType()
    {
        var (session, _, types) = _database.CreateStores();

        await using (session)
        {
            await types.SaveAsync(NewType("type-1"));
            Assert.Single(await types.ListAsync());

            await types.SaveAsync(NewType("type-2"));
            Assert.Equal(["type-1", "type-2"], (await types.ListAsync()).Select(type => type.WorkflowTypeId).Order());
            Assert.NotNull(await types.GetAsync("type-2"));
        }
    }

    [Fact]
    public async Task GetAsync_AfterADelete_ReturnsNull()
    {
        var (session, _, types) = _database.CreateStores();

        await using (session)
        {
            var type = NewType("type-1");
            await types.SaveAsync(type);
            Assert.NotNull(await types.GetAsync("type-1"));

            await types.DeleteAsync(type);

            Assert.Null(await types.GetAsync("type-1"));
        }
    }

    private static WorkflowType NewType(string workflowTypeId)
        => new()
        {
            WorkflowTypeId = workflowTypeId,
            Name = workflowTypeId,
            IsEnabled = true,
            Activities = [new ActivityRecord { ActivityId = "start", Name = "ScriptTask", IsStart = true }],
        };
}
