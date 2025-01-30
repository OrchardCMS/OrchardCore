using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        AutoroutePermissions.SetHomepage,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'AutoroutePermissions.SetHomepage'.")]
    public static readonly Permission SetHomepage = new("SetHomepage", "Set homepage.");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
