using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

/// <summary>
/// Represents the localization module permissions.
/// </summary>
public class Permissions : IPermissionProvider
{
    /// <summary>
    /// Gets a permission for managing the cultures.
    /// </summary>
    public static readonly Permission ManageCultures = new("ManageCultures", "Manage supported culture");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageCultures,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
       => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
