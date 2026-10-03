using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Placements;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManagePlacements = new("ManagePlacements", LocalizationSource.Create("Manage placements", typeof(Permissions)));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManagePlacements,
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
        },
    ];
}
