using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageWorkflows = new("ManageWorkflows", "Manage workflows", isSecurityCritical: true);
    public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", "Execute workflows", isSecurityCritical: true);
    public static readonly Permission ManageWorkflowSettings = new("ManageWorkflowSettings", "Manage workflow settings", [ManageWorkflows]);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageWorkflows,
        ExecuteWorkflows,
        ManageWorkflowSettings,
    ];

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
