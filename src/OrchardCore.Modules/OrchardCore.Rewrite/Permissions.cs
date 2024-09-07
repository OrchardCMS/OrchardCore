using OrchardCore.Security.Permissions;

namespace OrchardCore.Rewrite;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageRewrites = new Permission("ManageRewrites", "Manage rewrites");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageRewrites,
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
