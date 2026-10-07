using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

public sealed class WorkflowTypeDiffTests
{
    [Fact]
    public void Compare_SameDefinition_HasNoChanges()
    {
        var changes = WorkflowTypeDiff.Compare(CreateWorkflowType(), CreateWorkflowType());

        Assert.False(changes.HasChanges);
    }

    [Fact]
    public void Compare_ActivitiesAddedRemovedChangedAndMoved_ReportsEachKind()
    {
        var to = CreateWorkflowType();
        to.Activities.Remove(to.Activities.Single(activity => activity.ActivityId == "b"));
        to.Activities.Add(new ActivityRecord { ActivityId = "c", Name = "NotifyTask" });
        to.Activities.Single(activity => activity.ActivityId == "a").Properties["Message"] = "Changed";
        to.Activities.Single(activity => activity.ActivityId == "start").X = 40;

        var changes = WorkflowTypeDiff.Compare(CreateWorkflowType(), to);

        Assert.Equal(["c"], changes.AddedActivityIds);
        Assert.Equal(["b"], changes.RemovedActivityIds);
        Assert.Equal(["a"], changes.ChangedActivityIds);
        Assert.Equal(["start"], changes.MovedActivityIds);
        Assert.True(changes.HasChanges);
    }

    [Fact]
    public void Compare_ActivityChangedAndMoved_IsOnlyReportedAsChanged()
    {
        var to = CreateWorkflowType();
        var activity = to.Activities.Single(activity => activity.ActivityId == "a");
        activity.IsStart = true;
        activity.Y = 300;

        var changes = WorkflowTypeDiff.Compare(CreateWorkflowType(), to);

        Assert.Equal(["a"], changes.ChangedActivityIds);
        Assert.Empty(changes.MovedActivityIds);
    }

    [Fact]
    public void Compare_TransitionRewired_ReportsTheRemovedAndAddedTransitions()
    {
        var to = CreateWorkflowType();
        to.Transitions[0].DestinationActivityId = "b";

        var changes = WorkflowTypeDiff.Compare(CreateWorkflowType(), to);

        Assert.Equal(["start:Done:b"], changes.AddedTransitionKeys);
        Assert.Equal(["start:Done:a"], changes.RemovedTransitionKeys);
        Assert.Empty(changes.ChangedActivityIds);
    }

    [Fact]
    public void Compare_SettingsChanged_ListsTheirNames()
    {
        var to = CreateWorkflowType();
        to.Name = "Renamed";
        to.DeleteFinishedWorkflows = true;

        var changes = WorkflowTypeDiff.Compare(CreateWorkflowType(), to);

        Assert.Equal([nameof(WorkflowType.Name), nameof(WorkflowType.DeleteFinishedWorkflows)], changes.ChangedSettings);
    }

    [Fact]
    public void Compare_UsableAsActivityAndBranchingModeChanged_ListsThem()
    {
        var to = CreateWorkflowType();
        to.IsActivity = true;
        to.BranchingMode = WorkflowBranchingMode.All;

        var changes = WorkflowTypeDiff.Compare(CreateWorkflowType(), to);

        Assert.Equal([nameof(WorkflowType.IsActivity), nameof(WorkflowType.BranchingMode)], changes.ChangedSettings);
    }

    [Fact]
    public void Compare_VariableDefaultChanged_ListsTheVariables()
    {
        var from = CreateWorkflowType();
        from.Variables.Add(new WorkflowVariableDefinition { Name = "total", TypeName = "number", DefaultValue = 0 });
        var to = CreateWorkflowType();
        to.Variables.Add(new WorkflowVariableDefinition { Name = "total", TypeName = "number", DefaultValue = 1 });

        Assert.Equal([nameof(WorkflowType.Variables)], WorkflowTypeDiff.Compare(from, to).ChangedSettings);
        Assert.False(WorkflowTypeDiff.Compare(from, from).HasChanges);
    }

    private static WorkflowType CreateWorkflowType()
        => new()
        {
            WorkflowTypeId = "type-1",
            Name = "Diff",
            Activities =
            [
                new ActivityRecord { ActivityId = "start", Name = "SignalEvent", IsStart = true },
                new ActivityRecord { ActivityId = "a", Name = "NotifyTask", Properties = new JsonObject { ["Message"] = "Hello" } },
                new ActivityRecord { ActivityId = "b", Name = "NotifyTask" },
            ],
            Transitions =
            [
                new Transition { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "a" },
                new Transition { SourceActivityId = "a", SourceOutcomeName = "Done", DestinationActivityId = "b" },
            ],
        };
}
