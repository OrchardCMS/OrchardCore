using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

/// <summary>
/// Represents the localization module permissions.
/// </summary>
public class PermissionProvider : IPermissionProvider
{
    public PermissionProvider(IStringLocalizer<PermissionProvider> S)
    {
        Permissions.ManageCultures = new Permission(nameof(Permissions.ManageCultures), S["View audit trail"]);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync() => await Task.FromResult(new[] { Permissions.ManageCultures });

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = new [] { Permissions.ManageCultures },
        }
    ];

    public class Permissions
    {
        /// <summary>
        /// Gets a permission for managing the cultures.
        /// </summary>
        public static Permission ManageCultures { get; internal set; }
    }
}
