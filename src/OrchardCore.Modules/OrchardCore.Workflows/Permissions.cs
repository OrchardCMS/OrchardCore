using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageWorkflows = new("ManageWorkflows", "Manage workflows", isSecurityCritical: true);
    public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", "Execute workflows", isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageWorkflows,
        ExecuteWorkflows,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _allPermissions,
        }
    ];
}
