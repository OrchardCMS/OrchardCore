using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataLocalization;

/// <summary>
/// Represents the localization module permissions.
/// </summary>
public class Permissions : IPermissionProvider
{
    /// <summary>
    /// Gets a permission for managing the cultures.
    /// </summary>
    public static readonly Permission ManageLocalization = new("ManageLocalization", "Manage dynamic localizations");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageLocalization
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
