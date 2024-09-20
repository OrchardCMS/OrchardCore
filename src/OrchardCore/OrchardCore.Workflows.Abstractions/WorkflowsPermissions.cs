using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public sealed class WorkflowsPermissions
{
    public static readonly Permission ManageWorkflows = new("ManageWorkflows", "Manage workflows", isSecurityCritical: true);

    public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", "Execute workflows", isSecurityCritical: true);

    public static readonly Permission ManageWorkflowSettings = new("ManageWorkflowSettings", "Manage workflow settings", [ManageWorkflows]);
}
