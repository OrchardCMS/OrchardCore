using System.Text.Json.Nodes;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Composition;

// A parent workflow runs a child with the Execute Workflow task. The child doubles its "amount" input into its
// "doubled" output, and can wait on an event or fail on the way.
public sealed class ExecuteWorkflowTaskTests
{
    private readonly Dictionary<string, WorkflowType> _types = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Workflow> _instances = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<IActivity>> _activities = new(StringComparer.Ordinal);

    // The flags the parents reached, with the value their "result" variable had then.
    private readonly List<(string Flag, object Result)> _flags = [];
    private readonly WorkflowManager _manager;

    public ExecuteWorkflowTaskTests()
    {
        var workflowTypeStore = new Mock<IWorkflowTypeStore>();
        workflowTypeStore.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((string id) => _types.GetValueOrDefault(id));

        var versionStore = new Mock<IWorkflowTypeVersionStore>();
        versionStore.Setup(x => x.GetWorkflowTypeAsync(It.IsAny<WorkflowType>(), It.IsAny<string>())).ReturnsAsync((WorkflowType type, string _) => type);

        var workflowStore = new Mock<IWorkflowStore>();
        workflowStore.Setup(x => x.SaveAsync(It.IsAny<Workflow>())).Callback((Workflow workflow) => _instances[workflow.WorkflowId] = workflow).Returns(Task.CompletedTask);
        workflowStore.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((string id) => _instances.GetValueOrDefault(id));

        var idGenerator = new Mock<IWorkflowIdGenerator>();
        idGenerator.Setup(x => x.GenerateUniqueId(It.IsAny<Workflow>())).Returns(() => IdGenerator.GenerateId());

        var activityLibrary = new Mock<IActivityLibrary>();
        activityLibrary.Setup(x => x.InstantiateActivity(It.IsAny<string>())).Returns((string name) => _activities[name]());
        activityLibrary.Setup(x => x.GetActivityByName(It.IsAny<string>())).Returns((string name) => _activities[name]());

        var services = new ServiceCollection();
        services.AddSingleton<IWorkflowManager>(_ => _manager);
        var serviceProvider = services.BuildServiceProvider();

        // The instances are saved and loaded as the site does.
        var jsonOptions = new DocumentJsonSerializerOptions();
        new DocumentJsonSerializerOptionsConfiguration(Options.Create(new JsonDerivedTypesOptions())).Configure(jsonOptions);

        _manager = new WorkflowManager(
            activityLibrary.Object,
            workflowTypeStore.Object,
            versionStore.Object,
            TestVariableTypes.CreateProvider(),
            workflowStore.Object,
            Mock.Of<IWorkflowExecutionJournal>(),
            Mock.Of<IWorkflowDesignerNotifier>(),
            idGenerator.Object,
            new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider),
            Mock.Of<IWorkflowFaultHandler>(),
            Mock.Of<IDistributedLock>(),
            Mock.Of<ILogger<WorkflowManager>>(),
            Mock.Of<ILogger<MissingActivity>>(),
            Mock.Of<IStringLocalizer<MissingActivity>>(),
            Options.Create(jsonOptions),
            Mock.Of<IClock>());

        var expressions = TestExpressions.CreateManager();
        Register(() => new StartedByWorkflowEvent(new PassThroughStringLocalizer<StartedByWorkflowEvent>()));
        Register(() => new ExecuteWorkflowTask(workflowTypeStore.Object, expressions, serviceProvider, new PassThroughStringLocalizer<ExecuteWorkflowTask>()));
        Register(() => new DoubleTask());
        Register(() => new WaitTask());
        Register(() => new FailTask());
        Register(() => new FlagTask(_flags));
    }

    [Fact]
    public async Task Execute_ChildFinishes_ReturnsItsOutputs()
    {
        AddChild();
        var parentType = AddParent();

        var parent = await _manager.StartWorkflowAsync(parentType);

        Assert.Equal(WorkflowStatus.Finished, parent.Status);
        Assert.Equal<(string, object)>([("done", 42d)], _flags);

        var child = Assert.Single(_instances.Values, workflow => workflow.WorkflowTypeId == "child");
        Assert.Equal(WorkflowStatus.Finished, child.Status);
        Assert.Equal(parent.WorkflowId, child.ParentWorkflowId);
        Assert.Equal("exec", child.ParentActivityId);
    }

    [Fact]
    public async Task Execute_ChildWaits_TheParentWaitsAndResumesWhenTheChildFinishes()
    {
        AddChild(wait: true);
        var parentType = AddParent();

        var parent = await _manager.StartWorkflowAsync(parentType);

        Assert.Equal(WorkflowStatus.Halted, parent.Status);
        Assert.Equal("exec", Assert.Single(parent.Workflow.BlockingActivities).ActivityId);

        var child = Assert.Single(_instances.Values, workflow => workflow.WorkflowTypeId == "child");
        Assert.Equal(WorkflowStatus.Halted, child.Status);
        Assert.Empty(_flags);

        await _manager.ResumeWorkflowAsync(child, child.BlockingActivities.Single());

        Assert.Equal(WorkflowStatus.Finished, child.Status);
        Assert.Equal(WorkflowStatus.Finished, _instances[parent.WorkflowId].Status);
        Assert.Equal<(string, object)>([("done", 42d)], _flags);

        // The result isn't kept in the parent's input.
        Assert.False(_instances[parent.WorkflowId].State.ToObject<WorkflowState>().Input.ContainsKey(ChildWorkflowResult.InputKey));
    }

    [Fact]
    public async Task Execute_ChildFaults_TakesTheFailedOutcome()
    {
        AddChild(fail: true);
        var parentType = AddParent();

        var parent = await _manager.StartWorkflowAsync(parentType);

        Assert.Equal(WorkflowStatus.Finished, parent.Status);
        Assert.Equal<(string, object)>([("failed", null)], _flags);
        Assert.Equal("Simulated failure", parent.LastResult);
    }

    [Fact]
    public async Task Execute_WaitingChildFaultsLater_ResumesTheParentWithTheFailedOutcome()
    {
        AddChild(wait: true, fail: true);
        var parentType = AddParent();
        var parent = await _manager.StartWorkflowAsync(parentType);
        var child = Assert.Single(_instances.Values, workflow => workflow.WorkflowTypeId == "child");

        await _manager.ResumeWorkflowAsync(child, child.BlockingActivities.Single());

        Assert.Equal(WorkflowStatus.Faulted, child.Status);
        Assert.Equal(WorkflowStatus.Finished, _instances[parent.WorkflowId].Status);
        Assert.Equal<(string, object)>([("failed", null)], _flags);
    }

    [Fact]
    public async Task Execute_WithoutWaiting_TakesDoneAtOnceAndTheChildDoesNotResumeIt()
    {
        AddChild(wait: true);
        var parentType = AddParent(waitForCompletion: false);

        var parent = await _manager.StartWorkflowAsync(parentType);

        Assert.Equal(WorkflowStatus.Finished, parent.Status);
        Assert.Equal<(string, object)>([("done", null)], _flags);

        var child = Assert.Single(_instances.Values, workflow => workflow.WorkflowTypeId == "child");
        Assert.Equal(parent.WorkflowId, child.ParentWorkflowId);

        var resumedChild = await _manager.ResumeWorkflowAsync(child, child.BlockingActivities.Single());

        Assert.Equal(WorkflowStatus.Finished, resumedChild.Status);
        Assert.Single(_flags);
    }

    [Fact]
    public async Task Execute_WorkflowThatRunsItself_FailsAtTheDepthLimit()
    {
        var recursive = AddParent(workflowTypeId: "recursive", target: "recursive", isActivity: true, start: nameof(StartedByWorkflowEvent));

        var top = await _manager.StartWorkflowAsync(recursive);

        // The top instance and the children it runs, down to the limit, where the last one faults.
        Assert.Equal(WorkflowStatus.Finished, top.Status);
        Assert.Equal(WorkflowManager.MaxChildWorkflowDepth + 1, _instances.Count);

        var faulted = Assert.Single(_instances.Values, workflow => workflow.Status == WorkflowStatus.Faulted);
        Assert.Contains($"{WorkflowManager.MaxChildWorkflowDepth} levels deep", faulted.FaultMessage);
    }

    [Fact]
    public async Task Execute_WorkflowNotUsableAsAnActivity_Faults()
    {
        AddChild().IsActivity = false;
        var parentType = AddParent();

        var parent = await _manager.StartWorkflowAsync(parentType);

        Assert.Equal(WorkflowStatus.Faulted, parent.Status);
        Assert.Contains("isn't usable as an activity", parent.Workflow.FaultMessage);
        Assert.DoesNotContain(_instances.Values, workflow => workflow.WorkflowTypeId == "child");
    }

    [Fact]
    public void GetOutputs_StoredOutputs_AreTheTaskOutputs()
    {
        var task = (ExecuteWorkflowTask)_activities[nameof(ExecuteWorkflowTask)]();
        task.Outputs = [new ExecuteWorkflowOutput { Name = "doubled", TypeName = "number", Description = "Twice the amount" }];

        var output = Assert.Single(task.GetOutputs());

        Assert.Equal("doubled", output.Name);
        Assert.Equal("number", output.TypeName);
        Assert.Equal("doubled", output.DisplayName.Value);
        Assert.Equal("Twice the amount", output.Description.Value);
    }

    // start → double (→ wait) (→ fail): "amount" in, "doubled" out.
    private WorkflowType AddChild(bool wait = false, bool fail = false)
    {
        var activities = new List<ActivityRecord>
        {
            new() { ActivityId = "start", Name = nameof(StartedByWorkflowEvent), IsStart = true },
            new() { ActivityId = "double", Name = nameof(DoubleTask) },
        };
        var transitions = new List<Transition> { new() { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "double" } };
        var last = "double";

        if (wait)
        {
            activities.Add(new() { ActivityId = "wait", Name = nameof(WaitTask) });
            transitions.Add(new() { SourceActivityId = last, SourceOutcomeName = "Done", DestinationActivityId = "wait" });
            last = "wait";
        }

        if (fail)
        {
            activities.Add(new() { ActivityId = "fail", Name = nameof(FailTask) });
            transitions.Add(new() { SourceActivityId = last, SourceOutcomeName = "Done", DestinationActivityId = "fail" });
        }

        var workflowType = new WorkflowType
        {
            Id = 2,
            WorkflowTypeId = "child",
            Name = "Child",
            IsEnabled = true,
            IsActivity = true,
            Variables =
            [
                new() { Name = "amount", TypeName = "number", IsInput = true },
                new() { Name = "doubled", TypeName = "number", IsOutput = true },
            ],
            Activities = activities,
            Transitions = transitions,
        };

        _types[workflowType.WorkflowTypeId] = workflowType;

        return workflowType;
    }

    // start → exec (the target, with amount = 21; doubled → result) → done (Done) or failed (Failed).
    private WorkflowType AddParent(
        string workflowTypeId = "parent",
        string target = "child",
        bool waitForCompletion = true,
        bool isActivity = false,
        string start = nameof(StartedByWorkflowEvent))
    {
        var properties = new JsonObject
        {
            [nameof(ExecuteWorkflowTask.WorkflowTypeId)] = target,
            [nameof(ExecuteWorkflowTask.WaitForCompletion)] = waitForCompletion,
            [nameof(ExecuteWorkflowTask.Inputs)] = new JsonObject
            {
                ["amount"] = new JsonObject { ["Expression"] = "21", ["Syntax"] = WorkflowExpressionSyntaxes.Literal },
            },
        };
        properties.SetOutputBindings(new Dictionary<string, string> { ["doubled"] = "result" });

        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = workflowTypeId,
            Name = workflowTypeId,
            IsEnabled = true,
            IsActivity = isActivity,
            Variables =
            [
                new() { Name = "amount", TypeName = "number", IsInput = true },
                new() { Name = "doubled", TypeName = "number", IsOutput = true },
                new() { Name = "result", TypeName = "number" },
            ],
            Activities =
            [
                new() { ActivityId = "start", Name = start, IsStart = true },
                new() { ActivityId = "exec", Name = nameof(ExecuteWorkflowTask), Properties = properties },
                new() { ActivityId = "done", Name = nameof(FlagTask), Properties = new JsonObject { ["Flag"] = "done" } },
                new() { ActivityId = "failed", Name = nameof(FlagTask), Properties = new JsonObject { ["Flag"] = "failed" } },
            ],
            Transitions =
            [
                new() { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "exec" },
                new() { SourceActivityId = "exec", SourceOutcomeName = "Done", DestinationActivityId = "done" },
                new() { SourceActivityId = "exec", SourceOutcomeName = "Failed", DestinationActivityId = "failed" },
            ],
        };

        _types[workflowType.WorkflowTypeId] = workflowType;

        return workflowType;
    }

    private void Register<TActivity>(Func<TActivity> factory)
        where TActivity : IActivity
        => _activities[typeof(TActivity).Name] = () => factory();

    private abstract class TestTask<T> : TaskActivity<T>
        where T : ITask
    {
        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));
    }

    // Sets "doubled" to twice "amount".
    private sealed class DoubleTask : TestTask<DoubleTask>
    {
        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            workflowContext.Variables.Set("doubled", Convert.ToDouble(workflowContext.Variables["amount"], CultureInfo.InvariantCulture) * 2);

            return Outcome("Done");
        }
    }

    // Waits until the instance is resumed.
    private sealed class WaitTask : TestTask<WaitTask>
    {
        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Halt();

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome("Done");
    }

    private sealed class FailTask : TestTask<FailTask>
    {
        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => throw new InvalidOperationException("Simulated failure");
    }

    // Records its "Flag" property, with the value of the "result" variable.
    private sealed class FlagTask : TestTask<FlagTask>
    {
        private readonly List<(string Flag, object Result)> _flags;

        public FlagTask(List<(string Flag, object Result)> flags)
        {
            _flags = flags;
        }

        public string Flag => GetProperty<string>();

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _flags.Add((Flag, workflowContext.Variables["result"]));

            return Outcome("Done");
        }
    }
}
