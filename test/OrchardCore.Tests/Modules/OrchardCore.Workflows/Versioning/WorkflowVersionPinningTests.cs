using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

// Runs the real engine on the real workflow type and version stores: an instance waits on an event, a new
// version is published, and the instance resumes.
public sealed class WorkflowVersionPinningTests : IAsyncLifetime
{
    private readonly List<string> _executed = [];
    private VersioningTestDatabase _database;

    public async ValueTask InitializeAsync()
        => _database = await VersioningTestDatabase.CreateAsync();

    public ValueTask DisposeAsync()
        => _database?.DisposeAsync() ?? ValueTask.CompletedTask;

    [Fact]
    public async Task StartWorkflowAsync_PublishedType_PinsTheInstanceToItsVersion()
    {
        var workflow = await StartOnVersionOneAsync();

        var version = Assert.Single(await _database.CreateStores().Versions.ListAsync("type-1"));
        Assert.Equal(version.VersionId, workflow.WorkflowTypeVersionId);
        Assert.Equal(WorkflowStatus.Halted, workflow.Status);
        Assert.Equal("wait", Assert.Single(workflow.BlockingActivities).ActivityId);
    }

    [Fact]
    public async Task ResumeWorkflowAsync_PinnedToVersionOne_RunsVersionOneAfterVersionTwoIsPublished()
    {
        var workflow = await StartOnVersionOneAsync();
        await PublishAsync(ReplaceTheLastActivity);

        var context = await ResumeAsync(workflow);

        Assert.Equal(WorkflowStatus.Finished, context.Status);
        Assert.Equal(["a1"], _executed);
    }

    [Fact]
    public async Task ResumeWorkflowAsync_Unpinned_RunsTheCurrentDefinition()
    {
        var workflow = await StartOnVersionOneAsync();
        await PublishAsync(ReplaceTheLastActivity);

        // Like an instance created before workflow types had versions.
        workflow.WorkflowTypeVersionId = null;
        var context = await ResumeAsync(workflow);

        Assert.Equal(WorkflowStatus.Finished, context.Status);
        Assert.Equal(["a2"], _executed);
    }

    [Fact]
    public async Task ResumeWorkflowAsync_BlockingActivityNotInTheDefinition_FaultsTheInstance()
    {
        var workflow = await StartOnVersionOneAsync();
        await PublishAsync(workflowType =>
        {
            workflowType.Activities.Remove(workflowType.Activities.Single(activity => activity.ActivityId == "wait"));
            workflowType.Transitions = [new Transition { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "a1" }];
        });

        workflow.WorkflowTypeVersionId = null;
        var context = await ResumeAsync(workflow);

        Assert.Equal(WorkflowStatus.Faulted, context.Status);
        Assert.Contains("'wait'", workflow.FaultMessage);
        Assert.Empty(_executed);
    }

    private async Task<Workflow> StartOnVersionOneAsync()
    {
        var (session, _, types) = _database.CreateStores();
        await types.SaveAsync(new WorkflowType
        {
            WorkflowTypeId = "type-1",
            Name = "Pinning",
            IsEnabled = true,
            Activities =
            [
                new ActivityRecord { ActivityId = "start", Name = nameof(PinTestEvent), IsStart = true },
                new ActivityRecord { ActivityId = "wait", Name = nameof(PinTestEvent) },
                new ActivityRecord { ActivityId = "a1", Name = nameof(RecordTask) },
            ],
            Transitions =
            [
                new Transition { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "wait" },
                new Transition { SourceActivityId = "wait", SourceOutcomeName = "Done", DestinationActivityId = "a1" },
            ],
        });
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (managerSession, manager) = CreateWorkflowManager();
        var workflowType = await _database.CreateStores().Types.GetAsync("type-1");
        var context = await manager.StartWorkflowAsync(workflowType);
        await managerSession.SaveChangesAsync(TestContext.Current.CancellationToken);

        return context.Workflow;
    }

    private async Task PublishAsync(Action<WorkflowType> change)
    {
        var (session, _, types) = _database.CreateStores();
        var workflowType = await types.GetAsync("type-1");
        change(workflowType);
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<WorkflowExecutionContext> ResumeAsync(Workflow workflow)
    {
        var (_, manager) = CreateWorkflowManager();

        return await manager.ResumeWorkflowAsync(workflow, workflow.BlockingActivities.Single());
    }

    // Version 2 runs "a2" instead of "a1" after the event.
    private static void ReplaceTheLastActivity(WorkflowType workflowType)
    {
        workflowType.Activities.Remove(workflowType.Activities.Single(activity => activity.ActivityId == "a1"));
        workflowType.Activities.Add(new ActivityRecord { ActivityId = "a2", Name = nameof(RecordTask) });
        workflowType.Transitions[1].DestinationActivityId = "a2";
    }

    private (YesSql.ISession Session, WorkflowManager Manager) CreateWorkflowManager()
    {
        var (session, versions, types) = _database.CreateStores();

        var activityLibrary = new Mock<IActivityLibrary>();
        activityLibrary.Setup(x => x.InstantiateActivity(nameof(PinTestEvent))).Returns(() => new PinTestEvent());
        activityLibrary.Setup(x => x.InstantiateActivity(nameof(RecordTask))).Returns(() => new RecordTask(_executed));

        var workflowIdGenerator = new Mock<IWorkflowIdGenerator>();
        workflowIdGenerator.Setup(x => x.GenerateUniqueId(It.IsAny<Workflow>())).Returns(() => IdGenerator.GenerateId());

        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var manager = new WorkflowManager(
            activityLibrary.Object,
            types,
            versions,
            Mock.Of<IWorkflowStore>(),
            workflowIdGenerator.Object,
            new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider),
            Mock.Of<IWorkflowFaultHandler>(),
            Mock.Of<IDistributedLock>(),
            Mock.Of<ILogger<WorkflowManager>>(),
            Mock.Of<ILogger<MissingActivity>>(),
            Mock.Of<IStringLocalizer<MissingActivity>>(),
            Options.Create(_database.JsonOptions),
            Mock.Of<IClock>(x => x.UtcNow == VersioningTestDatabase.Now));

        return (session, manager);
    }

    private sealed class PinTestEvent : EventActivity
    {
        public override string Name => nameof(PinTestEvent);

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome("Done");
    }

    private sealed class RecordTask : TaskActivity<RecordTask>
    {
        private readonly List<string> _executed;

        public RecordTask(List<string> executed)
        {
            _executed = executed;
        }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _executed.Add(activityContext.ActivityRecord.ActivityId);

            return Outcome("Done");
        }
    }
}
