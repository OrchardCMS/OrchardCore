using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

public sealed class WorkflowVersionRetentionTests : IAsyncLifetime
{
    private VersioningTestDatabase _database;

    public async ValueTask InitializeAsync()
        => _database = await VersioningTestDatabase.CreateAsync();

    public ValueTask DisposeAsync()
        => _database?.DisposeAsync() ?? ValueTask.CompletedTask;

    [Fact]
    public async Task CreateIfChangedAsync_NoMaxCount_KeepsEveryVersion()
    {
        await PublishVersionsAsync(count: 5, maxCount: 0);

        Assert.Equal(new[] { 5, 4, 3, 2, 1 }, await ListVersionNumbersAsync());
    }

    [Fact]
    public async Task CreateIfChangedAsync_MaxCount_DeletesTheOlderVersions()
    {
        await PublishVersionsAsync(count: 4, maxCount: 2);

        Assert.Equal(new[] { 4, 3 }, await ListVersionNumbersAsync());
    }

    [Fact]
    public async Task CreateIfChangedAsync_MaxCount_KeepsTheVersionsInstancesRunOn()
    {
        await PublishVersionsAsync(count: 1, maxCount: 2);
        var firstVersionId = (await _database.CreateStores().Versions.GetLatestAsync("type-1")).VersionId;

        // An instance runs on version 1.
        var (session, _, _) = _database.CreateStores();
        await session.SaveAsync(
            new Workflow { WorkflowId = "instance-1", WorkflowTypeId = "type-1", WorkflowTypeVersionId = firstVersionId, Status = WorkflowStatus.Halted },
            cancellationToken: TestContext.Current.CancellationToken);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        await PublishVersionsAsync(count: 3, maxCount: 2, firstX: 100);

        Assert.Equal(new[] { 4, 3, 1 }, await ListVersionNumbersAsync());
    }

    // Saves the workflow type "count" times, moving an activity each time, so each save creates a version.
    private async Task PublishVersionsAsync(int count, int maxCount, int firstX = 0)
    {
        for (var i = 0; i < count; i++)
        {
            var (session, _, types) = _database.CreateStores(maxCount);
            var workflowType = await types.GetAsync("type-1") ?? new WorkflowType
            {
                WorkflowTypeId = "type-1",
                Name = "Retention",
                IsEnabled = true,
                Activities = [new ActivityRecord { ActivityId = "start", Name = "SignalEvent", IsStart = true }],
            };

            workflowType.Activities[0].X = firstX + i;
            await types.SaveAsync(workflowType);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    private async Task<int[]> ListVersionNumbersAsync()
        => (await _database.CreateStores().Versions.ListAsync("type-1")).Select(version => version.Version).ToArray();
}
