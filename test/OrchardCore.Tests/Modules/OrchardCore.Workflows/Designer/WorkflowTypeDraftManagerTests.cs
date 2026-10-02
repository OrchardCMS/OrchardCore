using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Serialization;
using YesSql.Sql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;

public sealed class WorkflowTypeDraftManagerTests : IAsyncLifetime
{
    private static readonly DateTime s_now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IWorkflowTypeStore> _workflowTypeStore = new();
    private readonly Mock<IActivityLibrary> _activityLibrary = new();
    private readonly Dictionary<string, Func<IActivity>> _activities = [];
    private readonly List<ISession> _sessions = [];
    private IStore _store;
    private string _tempFilename;
    private WorkflowType _workflowType;

    public async ValueTask InitializeAsync()
    {
        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(new Configuration().UseSqLite($"Data Source={_tempFilename};Cache=Shared"));

        var derivedOptions = new Mock<IOptions<JsonDerivedTypesOptions>>();
        derivedOptions.Setup(x => x.Value).Returns(new JsonDerivedTypesOptions());
        var jsonOptions = new DocumentJsonSerializerOptions();
        new DocumentJsonSerializerOptionsConfiguration(derivedOptions.Object).Configure(jsonOptions);
        _store.Configuration.ContentSerializer = new DefaultContentJsonSerializer(Options.Create(jsonOptions));

        await using (var session = _store.CreateSession())
        {
            var builder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync());
            await builder.CreateMapIndexTableAsync<WorkflowTypeDraftIndex>(table => table.Column<string>("WorkflowTypeId"));
            await session.SaveChangesAsync();
        }

        _store.RegisterIndexes<WorkflowTypeDraftIndexProvider>();

        Register(() => new ForkTask(new PassThroughStringLocalizer<ForkTask>()));
        Register(() => new DraftTestEvent());
        Register(() => new DraftTestTask());

        _workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = "workflow-type-1",
            Name = "Live",
            IsEnabled = true,
            Activities =
            [
                new ActivityRecord { ActivityId = "start", Name = nameof(DraftTestEvent), IsStart = true, X = 10, Y = 20 },
                new ActivityRecord
                {
                    ActivityId = "fork",
                    Name = nameof(ForkTask),
                    X = 200,
                    Y = 20,
                    Properties = new JsonObject { ["Forks"] = new JsonArray("A", "B") },
                },
                new ActivityRecord { ActivityId = "a", Name = nameof(DraftTestTask), X = 400, Y = 0 },
                new ActivityRecord { ActivityId = "b", Name = nameof(DraftTestTask), X = 400, Y = 100 },
            ],
            Transitions =
            [
                new Transition { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "fork" },
                new Transition { SourceActivityId = "fork", SourceOutcomeName = "A", DestinationActivityId = "a" },
                new Transition { SourceActivityId = "fork", SourceOutcomeName = "B", DestinationActivityId = "b" },
            ],
        };

        _workflowTypeStore.Setup(x => x.GetAsync(_workflowType.WorkflowTypeId)).ReturnsAsync(() => _workflowType);
        _activityLibrary.Setup(x => x.GetActivityByName(It.IsAny<string>()))
            .Returns((string name) => _activities.TryGetValue(name, out var factory) ? factory() : null);
        _activityLibrary.Setup(x => x.InstantiateActivity(It.IsAny<string>()))
            .Returns((string name) => _activities.TryGetValue(name, out var factory) ? factory() : null);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var session in _sessions)
        {
            await session.DisposeAsync();
        }

        _store?.Dispose();

        if (_tempFilename is not null && File.Exists(_tempFilename))
        {
            try
            {
                File.Delete(_tempFilename);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task GetOrCreateAsync_NoDraft_DeepCopiesLiveType()
    {
        var manager = CreateManager();

        var draft = await manager.GetOrCreateAsync(_workflowType);
        draft.Activities.Single(x => x.ActivityId == "fork").Properties["Forks"] = new JsonArray("C");
        draft.Activities.Single(x => x.ActivityId == "start").X = 999;
        draft.Transitions.Clear();

        Assert.Equal(0, draft.Revision);
        Assert.Equal(["A", "B"], ForksOf(_workflowType));
        Assert.Equal(10, _workflowType.Activities.Single(x => x.ActivityId == "start").X);
        Assert.Equal(3, _workflowType.Transitions.Count);
    }

    [Fact]
    public async Task SaveGraphAsync_MatchingRevision_IncrementsRevisionAndPersists()
    {
        var first = await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 0, GraphOf(_workflowType, ("a", 410, 10)));
        var second = await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 1, GraphOf(_workflowType, ("a", 420, 20)));
        var draft = await CreateManager().GetAsync(_workflowType.WorkflowTypeId);

        Assert.Equal(WorkflowTypeDraftStatus.Succeeded, first.Status);
        Assert.Equal(1, first.Revision);
        Assert.Equal(WorkflowTypeDraftStatus.Succeeded, second.Status);
        Assert.Equal(2, second.Revision);
        Assert.Equal(2, draft.Revision);
        Assert.Equal(s_now, draft.ModifiedUtc);
        Assert.Equal(420, draft.Activities.Single(x => x.ActivityId == "a").X);
        Assert.Equal(400, _workflowType.Activities.Single(x => x.ActivityId == "a").X);
    }

    [Fact]
    public async Task SaveGraphAsync_StaleRevision_ReturnsConflict()
    {
        await CreateManager(userName: "alice").SaveGraphAsync(_workflowType.WorkflowTypeId, 0, GraphOf(_workflowType, ("a", 410, 10)));
        await CreateManager(userName: "alice").SaveGraphAsync(_workflowType.WorkflowTypeId, 1, GraphOf(_workflowType, ("a", 420, 10)));

        var result = await CreateManager(userName: "bob").SaveGraphAsync(_workflowType.WorkflowTypeId, 1, GraphOf(_workflowType, ("a", 0, 0)));
        var draft = await CreateManager().GetAsync(_workflowType.WorkflowTypeId);

        Assert.Equal(WorkflowTypeDraftStatus.Conflict, result.Status);
        Assert.Equal(2, result.Revision);
        Assert.Equal("alice", result.ModifiedByUserName);
        Assert.Equal(s_now, result.ModifiedUtc);
        Assert.Equal(420, draft.Activities.Single(x => x.ActivityId == "a").X);
    }

    [Fact]
    public async Task SaveGraphAsync_NonZeroRevisionWithoutDraft_ReturnsConflictAndCreatesNoDraft()
    {
        var result = await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 3, GraphOf(_workflowType));

        Assert.Equal(WorkflowTypeDraftStatus.Conflict, result.Status);
        Assert.Equal(0, result.Revision);
        Assert.Null(await CreateManager().GetAsync(_workflowType.WorkflowTypeId));
    }

    [Fact]
    public async Task SaveGraphAsync_RemovedActivityAndDanglingTransition_DropsThem()
    {
        var update = GraphOf(_workflowType);
        update.RemovedActivityIds = ["b"];
        update.Transitions.Add(new Transition { SourceActivityId = "a", SourceOutcomeName = "Done", DestinationActivityId = "missing" });

        var result = await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 0, update);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(result.Draft.Activities, x => x.ActivityId == "b");
        Assert.Equal(
            ["start:Done:fork", "fork:A:a"],
            result.Draft.Transitions.Select(WorkflowDesignIssue.GetTransitionKey));
    }

    [Fact]
    public async Task AddActivityAsync_FirstEventWithoutStart_BecomesStartActivity()
    {
        _workflowType.Activities.Single(x => x.ActivityId == "start").IsStart = false;

        var firstEvent = await CreateManager().AddActivityAsync(_workflowType.WorkflowTypeId, 0, nameof(DraftTestEvent), 50, 60);
        var secondEvent = await CreateManager().AddActivityAsync(_workflowType.WorkflowTypeId, 1, nameof(DraftTestEvent), 70, 80);
        var task = await CreateManager().AddActivityAsync(_workflowType.WorkflowTypeId, 2, nameof(DraftTestTask), 90, 100);

        Assert.True(firstEvent.Activity.IsStart);
        Assert.Equal(50, firstEvent.Activity.X);
        Assert.Equal(60, firstEvent.Activity.Y);
        Assert.False(string.IsNullOrEmpty(firstEvent.Activity.ActivityId));
        Assert.False(secondEvent.Activity.IsStart);
        Assert.False(task.Activity.IsStart);
        Assert.Equal(3, task.Revision);
        Assert.Equal(7, (await CreateManager().GetAsync(_workflowType.WorkflowTypeId)).Activities.Count);
    }

    [Fact]
    public async Task AddActivityAsync_UnknownActivity_ReturnsInvalidAndCreatesNoDraft()
    {
        var result = await CreateManager().AddActivityAsync(_workflowType.WorkflowTypeId, 0, "UnknownActivity", 0, 0);

        Assert.Equal(WorkflowTypeDraftStatus.Invalid, result.Status);
        Assert.Null(await CreateManager().GetAsync(_workflowType.WorkflowTypeId));
    }

    [Fact]
    public async Task UpdateActivityAsync_ForkLosesBranch_RemovesTransitionsOfVanishedOutcomes()
    {
        var properties = new JsonObject { ["Forks"] = new JsonArray("A", "C") };

        var result = await CreateManager().UpdateActivityAsync(_workflowType.WorkflowTypeId, 0, "fork", properties);

        Assert.True(result.Succeeded);
        Assert.Equal(["fork:B:b"], result.RemovedTransitions.Select(WorkflowDesignIssue.GetTransitionKey));
        Assert.Equal(["A", "C"], result.Activity.Properties["Forks"].AsArray().Select(x => x.GetValue<string>()));
        Assert.Equal(
            ["start:Done:fork", "fork:A:a"],
            result.Draft.Transitions.Select(WorkflowDesignIssue.GetTransitionKey));
        Assert.Equal(["A", "B"], ForksOf(_workflowType));
    }

    [Fact]
    public async Task UpdateActivityAsync_UnknownActivityId_ReturnsNotFound()
    {
        var result = await CreateManager().UpdateActivityAsync(_workflowType.WorkflowTypeId, 0, "missing", []);

        Assert.Equal(WorkflowTypeDraftStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateSettingsAsync_ThenPublish_AppliesSettingsToLiveType()
    {
        var settings = new WorkflowTypeDraftSettings
        {
            Name = " Renamed ",
            IsEnabled = false,
            IsSingleton = true,
            LockTimeout = 5,
            LockExpiration = 6,
            DeleteFinishedWorkflows = true,
        };

        var updated = await CreateManager().UpdateSettingsAsync(_workflowType.WorkflowTypeId, 0, settings);
        Assert.Equal("Live", _workflowType.Name);

        var published = await CreateManager().PublishAsync(_workflowType.WorkflowTypeId, updated.Revision);

        Assert.True(published.Succeeded);
        Assert.Equal("Renamed", _workflowType.Name);
        Assert.False(_workflowType.IsEnabled);
        Assert.True(_workflowType.IsSingleton);
        Assert.Equal(5, _workflowType.LockTimeout);
        Assert.Equal(6, _workflowType.LockExpiration);
        Assert.True(_workflowType.DeleteFinishedWorkflows);
    }

    [Fact]
    public async Task PublishAsync_CurrentRevision_SavesThroughStoreOnceAndDeletesDraft()
    {
        var saved = await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 0, GraphOf(_workflowType, ("a", 500, 50)));

        var manager = CreateManager();
        var result = await manager.PublishAsync(_workflowType.WorkflowTypeId, saved.Revision);
        await _sessions[^1].SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Same(_workflowType, result.WorkflowType);
        Assert.Equal(500, _workflowType.Activities.Single(x => x.ActivityId == "a").X);
        _workflowTypeStore.Verify(x => x.SaveAsync(_workflowType), Times.Once);
        Assert.Null(await CreateManager().GetAsync(_workflowType.WorkflowTypeId));
    }

    [Fact]
    public async Task PublishAsync_StaleRevision_ReturnsConflictAndKeepsLiveType()
    {
        await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 0, GraphOf(_workflowType, ("a", 500, 50)));

        var result = await CreateManager().PublishAsync(_workflowType.WorkflowTypeId, 0);

        Assert.Equal(WorkflowTypeDraftStatus.Conflict, result.Status);
        Assert.Equal(1, result.Revision);
        _workflowTypeStore.Verify(x => x.SaveAsync(It.IsAny<WorkflowType>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_DraftWithErrors_ReturnsInvalidAndKeepsLiveType()
    {
        // A legacy live type with a dangling transition: the draft copy keeps it until the graph is saved.
        _workflowType.Transitions.Add(new Transition { SourceActivityId = "a", SourceOutcomeName = "Done", DestinationActivityId = "missing" });
        var added = await CreateManager().AddActivityAsync(_workflowType.WorkflowTypeId, 0, nameof(DraftTestTask), 0, 0);

        var result = await CreateManager().PublishAsync(_workflowType.WorkflowTypeId, added.Revision);

        Assert.Equal(WorkflowTypeDraftStatus.Invalid, result.Status);
        Assert.Contains(result.Issues, x => x.Code == WorkflowDesignerConstants.IssueCodes.InvalidTransition);
        _workflowTypeStore.Verify(x => x.SaveAsync(It.IsAny<WorkflowType>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_NoDraft_ReturnsNotFound()
    {
        var result = await CreateManager().PublishAsync(_workflowType.WorkflowTypeId, 0);

        Assert.Equal(WorkflowTypeDraftStatus.NotFound, result.Status);
        _workflowTypeStore.Verify(x => x.SaveAsync(It.IsAny<WorkflowType>()), Times.Never);
    }

    [Fact]
    public async Task DiscardAsync_ExistingDraft_DeletesDraftAndKeepsLiveType()
    {
        await CreateManager().SaveGraphAsync(_workflowType.WorkflowTypeId, 0, GraphOf(_workflowType, ("a", 500, 50)));

        await CreateManager().DiscardAsync(_workflowType.WorkflowTypeId);
        await _sessions[^1].SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Null(await CreateManager().GetAsync(_workflowType.WorkflowTypeId));
        Assert.Equal(400, _workflowType.Activities.Single(x => x.ActivityId == "a").X);
        _workflowTypeStore.Verify(x => x.SaveAsync(It.IsAny<WorkflowType>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_ValidWorkflow_ReturnsNoIssues()
    {
        var issues = await CreateManager().ValidateAsync(_workflowType);

        Assert.Empty(issues);
    }

    [Fact]
    public async Task ValidateAsync_NoStartActivity_ReturnsMissingStartWarning()
    {
        _workflowType.Activities.Single(x => x.ActivityId == "start").IsStart = false;

        var issue = Assert.Single(await CreateManager().ValidateAsync(_workflowType));

        Assert.Equal(WorkflowDesignerConstants.IssueCodes.MissingStartActivity, issue.Code);
        Assert.Equal(WorkflowDesignIssueSeverity.Warning, issue.Severity);
    }

    [Fact]
    public async Task ValidateAsync_UnregisteredActivityType_ReturnsMissingActivityWarning()
    {
        _workflowType.Activities.Single(x => x.ActivityId == "b").Name = "NotRegisteredTask";

        var issue = Assert.Single(await CreateManager().ValidateAsync(_workflowType));

        Assert.Equal(WorkflowDesignerConstants.IssueCodes.MissingActivity, issue.Code);
        Assert.Equal(WorkflowDesignIssueSeverity.Warning, issue.Severity);
        Assert.Equal("b", issue.ActivityId);
    }

    [Fact]
    public async Task ValidateAsync_TransitionToMissingActivity_ReturnsInvalidTransitionError()
    {
        _workflowType.Transitions.Add(new Transition { SourceActivityId = "a", SourceOutcomeName = "Done", DestinationActivityId = "missing" });

        var issue = Assert.Single(await CreateManager().ValidateAsync(_workflowType));

        Assert.Equal(WorkflowDesignerConstants.IssueCodes.InvalidTransition, issue.Code);
        Assert.Equal(WorkflowDesignIssueSeverity.Error, issue.Severity);
        Assert.Equal("a:Done:missing", issue.TransitionKey);
    }

    [Fact]
    public async Task ValidateAsync_OutcomeWithTwoTransitions_ReturnsDuplicateOutcomeWarning()
    {
        _workflowType.Transitions.Add(new Transition { SourceActivityId = "fork", SourceOutcomeName = "A", DestinationActivityId = "b" });

        var issue = Assert.Single(await CreateManager().ValidateAsync(_workflowType));

        Assert.Equal(WorkflowDesignerConstants.IssueCodes.DuplicateOutcomeTransition, issue.Code);
        Assert.Equal(WorkflowDesignIssueSeverity.Warning, issue.Severity);
        Assert.Equal("fork", issue.ActivityId);
        Assert.Equal("fork:A:b", issue.TransitionKey);
    }

    [Fact]
    public async Task ValidateAsync_ActivityWithoutPathFromStart_ReturnsUnreachableWarning()
    {
        _workflowType.Activities.Add(new ActivityRecord { ActivityId = "orphan", Name = nameof(DraftTestTask) });

        var issue = Assert.Single(await CreateManager().ValidateAsync(_workflowType));

        Assert.Equal(WorkflowDesignerConstants.IssueCodes.UnreachableActivity, issue.Code);
        Assert.Equal(WorkflowDesignIssueSeverity.Warning, issue.Severity);
        Assert.Equal("orphan", issue.ActivityId);
    }

    private WorkflowTypeDraftManager CreateManager(string userName = "admin")
    {
        var session = _store.CreateSession();
        _sessions.Add(session);

        var workflowManager = new Mock<IWorkflowManager>();
        workflowManager.Setup(x => x.NewWorkflow(It.IsAny<WorkflowType>(), It.IsAny<string>()))
            .Returns((WorkflowType type, string _) => new Workflow { WorkflowId = "workflow", WorkflowTypeId = type.WorkflowTypeId });
        workflowManager.Setup(x => x.CreateWorkflowExecutionContextAsync(It.IsAny<WorkflowType>(), It.IsAny<Workflow>(), It.IsAny<IDictionary<string, object>>()))
            .ReturnsAsync((WorkflowType type, Workflow workflow, IDictionary<string, object> _) => new WorkflowExecutionContext(type, workflow, null, null, null, null, null, []));
        workflowManager.Setup(x => x.CreateActivityExecutionContextAsync(It.IsAny<ActivityRecord>(), It.IsAny<JsonObject>()))
            .ReturnsAsync((ActivityRecord record, JsonObject properties) =>
            {
                var activity = _activities[record.Name]();
                activity.Properties = properties;

                return new ActivityContext { ActivityRecord = record, Activity = activity };
            });

        var activityIdGenerator = new Mock<IActivityIdGenerator>();
        activityIdGenerator.Setup(x => x.GenerateUniqueId(It.IsAny<ActivityRecord>())).Returns(() => IdGenerator.GenerateId());

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userName + "-id"), new Claim(ClaimTypes.Name, userName)],
                "Test")),
        };

        return new WorkflowTypeDraftManager(
            session,
            _workflowTypeStore.Object,
            workflowManager.Object,
            _activityLibrary.Object,
            activityIdGenerator.Object,
            Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
            Mock.Of<IClock>(x => x.UtcNow == s_now),
            new PassThroughStringLocalizer<WorkflowTypeDraftManager>());
    }

    private void Register(Func<IActivity> factory)
        => _activities[factory().Name] = factory;

    private static WorkflowGraphUpdate GraphOf(WorkflowType workflowType, params (string Id, int X, int Y)[] moves)
        => new()
        {
            Nodes = workflowType.Activities
                .Select(activity =>
                {
                    var move = moves.FirstOrDefault(x => x.Id == activity.ActivityId);

                    return new WorkflowGraphNode
                    {
                        Id = activity.ActivityId,
                        X = move.Id is null ? activity.X : move.X,
                        Y = move.Id is null ? activity.Y : move.Y,
                        IsStart = activity.IsStart,
                    };
                })
                .ToList(),
            Transitions = workflowType.Transitions
                .Select(x => new Transition { SourceActivityId = x.SourceActivityId, SourceOutcomeName = x.SourceOutcomeName, DestinationActivityId = x.DestinationActivityId })
                .ToList(),
        };

    private static string[] ForksOf(WorkflowType workflowType)
        => workflowType.Activities.Single(x => x.ActivityId == "fork").Properties["Forks"].AsArray().Select(x => x.GetValue<string>()).ToArray();

    private sealed class DraftTestEvent : EventActivity
    {
        public override string Name => nameof(DraftTestEvent);

        public override LocalizedString DisplayText => new(nameof(DraftTestEvent), "Draft test event");

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));
    }

    private sealed class DraftTestTask : TaskActivity<DraftTestTask>
    {
        public override LocalizedString DisplayText => new(nameof(DraftTestTask), "Draft test task");

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));
    }

    private sealed class PassThroughStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
