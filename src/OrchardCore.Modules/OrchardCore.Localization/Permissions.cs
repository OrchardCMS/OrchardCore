using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

/// <summary>
/// Represents the localization module permissions.
/// </summary>
public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        LocalizationPermissions.ManageCultures,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'LocalizationPermissions.ManageCultures'.")]
    public static readonly Permission ManageCultures = LocalizationPermissions.ManageCultures;

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
