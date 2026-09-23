using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Themes;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ApplyTheme = new("ApplyTheme", LocalizedString.Create("Apply a Theme", typeof(Permissions)));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ApplyTheme,
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
