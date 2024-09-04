using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission SetHomepage = new("SetHomepage", "Set homepage.");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        SetHomepage,
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
    ];
}
