using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Workflows;

public class WorkflowTypeExtensionsTests
{
    [Fact]
    public void HasStartActivityShouldBeFalseWhenThereAreNoActivities()
    {
        var workflowType = new WorkflowType();

        Assert.False(workflowType.HasStartActivity());
    }

    [Fact]
    public void HasStartActivityShouldBeFalseWhenNoActivityIsStart()
    {
        var workflowType = new WorkflowType
        {
            Activities = [new ActivityRecord(), new ActivityRecord()],
        };

        Assert.False(workflowType.HasStartActivity());
    }

    [Fact]
    public void HasStartActivityShouldBeTrueWhenAnActivityIsStart()
    {
        var workflowType = new WorkflowType
        {
            Activities = [new ActivityRecord(), new ActivityRecord { IsStart = true }],
        };

        Assert.True(workflowType.HasStartActivity());
    }

    [Fact]
    public void IsMissingStartActivityShouldBeFalseWhenThereAreNoActivities()
    {
        var workflowType = new WorkflowType();

        Assert.False(workflowType.IsMissingStartActivity());
    }

    [Fact]
    public void IsMissingStartActivityShouldBeTrueWhenNoActivityIsStart()
    {
        var workflowType = new WorkflowType
        {
            Activities = [new ActivityRecord(), new ActivityRecord()],
        };

        Assert.True(workflowType.IsMissingStartActivity());
    }

    [Fact]
    public void IsMissingStartActivityShouldBeFalseWhenAnActivityIsStart()
    {
        var workflowType = new WorkflowType
        {
            Activities = [new ActivityRecord(), new ActivityRecord { IsStart = true }],
        };

        Assert.False(workflowType.IsMissingStartActivity());
    }
}
