using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public class Permissions : IPermissionProvider
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
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
        },
        new PermissionStereotype
        {
            Name = "Moderator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Author",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        AccessAdminPanel,
    ];
}
