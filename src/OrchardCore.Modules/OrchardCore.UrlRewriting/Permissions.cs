using OrchardCore.Security.Permissions;

namespace OrchardCore.UrlRewriting;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageUrlRewriting = new Permission("ManageUrlRewriting", "Manage URLs rewrites");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageUrlRewriting,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        }
    ];
}
