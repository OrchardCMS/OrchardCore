using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        WorkflowsPermissions.ManageWorkflows,
        WorkflowsPermissions.ExecuteWorkflows,
        WorkflowsPermissions.ManageWorkflowSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'WorkflowsPermissions.ManageWorkflows'.")]
    public static readonly Permission ManageWorkflows = WorkflowsPermissions.ManageWorkflows;

    [Obsolete("This will be removed in a future release. Instead use 'WorkflowsPermissions.ExecuteWorkflows'.")]
    public static readonly Permission ExecuteWorkflows = WorkflowsPermissions.ExecuteWorkflows;

    [Obsolete("This will be removed in a future release. Instead use 'WorkflowsPermissions.ManageWorkflowSettings'.")]
    public static readonly Permission ManageWorkflowSettings = WorkflowsPermissions.ManageWorkflowSettings;

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        }
    ];
}
