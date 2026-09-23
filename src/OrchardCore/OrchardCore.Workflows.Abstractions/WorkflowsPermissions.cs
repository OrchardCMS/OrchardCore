using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public static class WorkflowsPermissions
{
    public static readonly Permission ManageWorkflows = new("ManageWorkflows", LocalizedString.Create("Manage workflows", typeof(WorkflowsPermissions)), isSecurityCritical: true);

    public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", LocalizedString.Create("Execute workflows", typeof(WorkflowsPermissions)), isSecurityCritical: true);

    public static readonly Permission ManageWorkflowSettings = new("ManageWorkflowSettings", LocalizedString.Create("Manage workflow settings", typeof(WorkflowsPermissions)), [ManageWorkflows]);
}
