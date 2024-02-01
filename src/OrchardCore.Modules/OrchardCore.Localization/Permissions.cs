using System;
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
    private static Lazy<Permission> _manageCultures;

    private readonly IStringLocalizer S;

    public Permissions(IStringLocalizer<Permissions> localizer)
    {
        S = localizer;
        _manageCultures = new Lazy<Permission>(() => new("ManageCultures", S["Manage supported culture"]));
    }

    /// <summary>
    /// Gets a permission for managing the cultures.
    /// </summary>
    public static Permission ManageCultures => _manageCultures.Value;

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
       => Task.FromResult<IEnumerable<Permission>>(new[] { ManageCultures });

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = new [] { ManageCultures },
        }
    ];
}
