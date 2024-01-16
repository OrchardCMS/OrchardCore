using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageSettings = new("ManageSettings", "Manage settings");

    // This permission is not exposed, it's just used for the APIs to generate/check custom ones.
    public static readonly Permission ManageGroupSettings = new("ManageResourceSettings", "Manage settings", new[] { ManageSettings });

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSettings,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
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
        => _stereotypes;
}
