using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;
using OrchardCore.Workflows.Handlers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Trimming.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Journal;

public sealed class WorkflowExecutionJournalTests : IAsyncLifetime
{
    private VersioningTestDatabase _database;

    public async ValueTask InitializeAsync()
        => _database = await VersioningTestDatabase.CreateAsync();

    public ValueTask DisposeAsync()
        => _database.DisposeAsync();

    [Fact]
    public async Task SaveAsync_Records_AreListedInTheirOrder()
    {
        await SaveAsync("workflow-1", 1, 2, 3);
        await SaveAsync("workflow-2", 1);

        var records = await ListAsync("workflow-1");

        Assert.Equal([1, 2, 3], records.Select(record => record.Sequence));
        Assert.Equal(["a1", "a2", "a3"], records.Select(record => record.ActivityId));
        Assert.Equal(WorkflowExecutionRecordStatus.Completed, records[0].Status);
        Assert.Equal(["Done"], records[0].Outcomes);
    }

    [Fact]
    public async Task ListAsync_Count_ReturnsTheMostRecentRecords()
    {
        await SaveAsync("workflow-1", 1, 2, 3, 4);

        await using var session = _database.Store.CreateSession();
        var records = await VersioningTestDatabase.CreateJournal(session).ListAsync("workflow-1", 2);

        Assert.Equal([3, 4], records.Select(record => record.Sequence));
    }

    [Fact]
    public async Task SaveAsync_BeyondTheMaximum_DeletesTheOldestRecords()
    {
        await SaveAsync("workflow-1", 1, 2, 3);
        await SaveWithMaximumAsync("workflow-1", 3, 4, 5);

        Assert.Equal([3, 4, 5], (await ListAsync("workflow-1")).Select(record => record.Sequence));
    }

    [Fact]
    public async Task DeleteAsync_Instances_DeletesOnlyTheirRecords()
    {
        await SaveAsync("workflow-1", 1, 2);
        await SaveAsync("workflow-2", 1);
        await SaveAsync("workflow-3", 1);

        await using (var session = _database.Store.CreateSession())
        {
            await VersioningTestDatabase.CreateJournal(session).DeleteAsync(["workflow-1", "workflow-3"]);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(await ListAsync("workflow-1"));
        Assert.Single(await ListAsync("workflow-2"));
        Assert.Empty(await ListAsync("workflow-3"));
    }

    [Fact]
    public async Task WorkflowTypeStoreDeleteAsync_WorkflowType_DeletesTheJournalOfItsInstances()
    {
        await using (var session = _database.Store.CreateSession())
        {
            await session.SaveAsync(new WorkflowType { WorkflowTypeId = "type-1", Name = "Type", Activities = [], Transitions = [] }, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveAsync(new Workflow { WorkflowId = "workflow-1", WorkflowTypeId = "type-1" }, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await SaveAsync("workflow-1", 1);
        await SaveAsync("workflow-other", 1);

        var (stores, _, types) = _database.CreateStores();
        await types.DeleteAsync(await types.GetAsync("type-1"));
        await stores.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await ListAsync("workflow-1"));
        Assert.Single(await ListAsync("workflow-other"));
    }

    [Fact]
    public async Task TrimWorkflowInstancesAsync_OldInstances_DeletesTheirJournal()
    {
        await using (var session = _database.Store.CreateSession())
        {
            await session.SaveAsync(new Workflow { WorkflowId = "old", WorkflowTypeId = "type-1", Status = WorkflowStatus.Finished, CreatedUtc = VersioningTestDatabase.Now.AddDays(-40) }, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveAsync(new Workflow { WorkflowId = "recent", WorkflowTypeId = "type-1", Status = WorkflowStatus.Finished, CreatedUtc = VersioningTestDatabase.Now }, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await SaveAsync("old", 1);
        await SaveAsync("recent", 1);

        await using (var session = _database.Store.CreateSession())
        {
            var siteService = Mock.Of<ISiteService>(x => x.GetSiteSettingsAsync() == Task.FromResult<ISite>(new SiteSettings()));
            var trimming = new WorkflowTrimmingService(siteService, session, Mock.Of<IClock>(x => x.UtcNow == VersioningTestDatabase.Now), VersioningTestDatabase.CreateJournal(session));

            Assert.Equal(1, await trimming.TrimWorkflowInstancesAsync(TimeSpan.FromDays(30), 10));
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(await ListAsync("old"));
        Assert.Single(await ListAsync("recent"));
    }

    [Fact]
    public async Task WorkflowJournalHandler_DeletedInstance_DeletesItsJournal()
    {
        var journal = new Mock<IWorkflowExecutionJournal>();

        await new WorkflowJournalHandler(journal.Object).DeletedAsync(new WorkflowDeletedContext(new Workflow { WorkflowId = "workflow-1" }));

        journal.Verify(x => x.DeleteAsync(It.Is<IEnumerable<string>>(ids => ids.Single() == "workflow-1")), Times.Once);
    }

    private Task SaveAsync(string workflowId, params int[] sequences)
        => SaveWithMaximumAsync(workflowId, 1000, sequences);

    private async Task SaveWithMaximumAsync(string workflowId, int maxRecords, params int[] sequences)
    {
        await using var session = _database.Store.CreateSession();

        await VersioningTestDatabase.CreateJournal(session, maxRecords).SaveAsync(workflowId, sequences.Select(sequence => new WorkflowExecutionRecord
        {
            WorkflowId = workflowId,
            WorkflowTypeId = "type-1",
            Sequence = sequence,
            ActivityId = $"a{sequence}",
            ActivityName = "NotifyTask",
            Status = WorkflowExecutionRecordStatus.Completed,
            Outcomes = ["Done"],
        }));

        await session.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowExecutionRecord>> ListAsync(string workflowId)
    {
        await using var session = _database.Store.CreateSession();

        return await VersioningTestDatabase.CreateJournal(session).ListAsync(workflowId);
    }
}
