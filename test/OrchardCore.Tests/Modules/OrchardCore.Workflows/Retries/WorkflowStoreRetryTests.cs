using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Retries;

public sealed class WorkflowStoreRetryTests : IAsyncLifetime
{
    private static readonly DateTime _now = VersioningTestDatabase.Now;

    private VersioningTestDatabase _database;

    public async ValueTask InitializeAsync()
        => _database = await VersioningTestDatabase.CreateAsync();

    public ValueTask DisposeAsync()
        => _database.DisposeAsync();

    [Fact]
    public async Task ListDueRetriesAsync_Instances_ListsTheFaultedOnesWhoseRetryIsDueEarliestFirst()
    {
        var (session, _, _) = _database.CreateStores();

        await using (session)
        {
            var store = new WorkflowStore(session, [], NullLogger<WorkflowStore>.Instance);

            await store.SaveAsync(Faulted("due-later", _now.AddSeconds(-10)));
            await store.SaveAsync(Faulted("due-first", _now.AddMinutes(-5)));
            await store.SaveAsync(Faulted("not-due", _now.AddMinutes(1)));
            await store.SaveAsync(Faulted("no-retry", dueUtc: null));

            // A retry that's no longer pending once the instance runs again.
            var running = Faulted("running", _now.AddMinutes(-1));
            running.Status = WorkflowStatus.Halted;
            await store.SaveAsync(running);

            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var (listing, _, _) = _database.CreateStores();
        var due = await new WorkflowStore(listing, [], NullLogger<WorkflowStore>.Instance).ListDueRetriesAsync(_now, take: 10);

        Assert.Equal(["due-first", "due-later"], due.Select(workflow => workflow.WorkflowId));
    }

    private static Workflow Faulted(string workflowId, DateTime? dueUtc)
        => new()
        {
            WorkflowId = workflowId,
            WorkflowTypeId = "type-1",
            Status = WorkflowStatus.Faulted,
            CreatedUtc = _now,
            PendingRetry = dueUtc is null ? null : new WorkflowPendingRetry { ActivityId = "task", FailedAttempts = 1, MaxRetries = 3, DueUtc = dueUtc.Value },
        };
}
