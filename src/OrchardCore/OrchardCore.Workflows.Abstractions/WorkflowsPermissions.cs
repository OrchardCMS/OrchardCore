using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public static class WorkflowsPermissions
{
    public static readonly Permission ManageWorkflows = new("ManageWorkflows", LocalizationSource.Create("Manage workflows", typeof(WorkflowsPermissions)), isSecurityCritical: true);

    public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", LocalizationSource.Create("Execute workflows", typeof(WorkflowsPermissions)), isSecurityCritical: true);

    public static readonly Permission ManageWorkflowSettings = new("ManageWorkflowSettings", LocalizationSource.Create("Manage workflow settings", typeof(WorkflowsPermissions)), [ManageWorkflows]);
}
