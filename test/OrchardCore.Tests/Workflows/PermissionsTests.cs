using OrchardCore.Workflows;

namespace OrchardCore.Tests.Workflows;

public class PermissionsTests
{
    [Fact]
    public void Administrator_ReceivesAllWorkflowPermissions()
    {
        var administrator = new Permissions()
            .GetDefaultStereotypes()
            .Single(stereotype => stereotype.Name == OrchardCoreConstants.Roles.Administrator);

        Assert.Contains(WorkflowsPermissions.ManageWorkflows, administrator.Permissions);
        Assert.Contains(WorkflowsPermissions.ExecuteWorkflows, administrator.Permissions);
        Assert.Contains(WorkflowsPermissions.ManageWorkflowSettings, administrator.Permissions);
    }

    [Fact]
    public void Editor_ReceivesOnlyExecuteWorkflows()
    {
        var editor = new Permissions()
            .GetDefaultStereotypes()
            .Single(stereotype => stereotype.Name == OrchardCoreConstants.Roles.Editor);

        Assert.Contains(WorkflowsPermissions.ExecuteWorkflows, editor.Permissions);
        Assert.DoesNotContain(WorkflowsPermissions.ManageWorkflows, editor.Permissions);
        Assert.DoesNotContain(WorkflowsPermissions.ManageWorkflowSettings, editor.Permissions);
    }
}
