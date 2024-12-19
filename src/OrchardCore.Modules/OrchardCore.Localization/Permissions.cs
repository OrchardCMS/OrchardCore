using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

/// <summary>
/// Represents the localization module permissions.
/// </summary>
public sealed class Permissions : IPermissionProvider
{
    /// <summary>
    /// Gets a permission for managing the cultures.
    /// </summary>
    public static readonly Permission ManageCultures = new("ManageCultures", "Manage supported culture");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageCultures,
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
