using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageBackgroundTasks = new("ManageBackgroundTasks", "Manage background tasks");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageBackgroundTasks,
    ];

    private static readonly IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;
}
