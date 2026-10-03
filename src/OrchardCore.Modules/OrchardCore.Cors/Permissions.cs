using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Cors;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageCorsSettings = new("ManageCorsSettings", new LocalizationSource("Managing Cors Settings", typeof(Permissions)), isSecurityCritical: true);


    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageCorsSettings,
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
