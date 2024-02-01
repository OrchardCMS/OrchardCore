using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
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
    public static readonly Permission ManageCultures = new("ManageCultures");

    private readonly IStringLocalizer S;
    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageCultures,
    ];

    public Permissions(IStringLocalizer<Permissions> localizer)
    {
        S = localizer;
        ManageCultures.Description = S["Manage supported culture"];
    }

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
