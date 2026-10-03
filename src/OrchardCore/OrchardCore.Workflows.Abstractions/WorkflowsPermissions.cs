using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public static class WorkflowsPermissions
{
    public static readonly Permission ManageWorkflows = new("ManageWorkflows", new LocalizationSource("Manage workflows", typeof(WorkflowsPermissions)), isSecurityCritical: true);

    public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", new LocalizationSource("Execute workflows", typeof(WorkflowsPermissions)), isSecurityCritical: true);

    public static readonly Permission ManageWorkflowSettings = new("ManageWorkflowSettings", new LocalizationSource("Manage workflow settings", typeof(WorkflowsPermissions)), [ManageWorkflows]);
}
