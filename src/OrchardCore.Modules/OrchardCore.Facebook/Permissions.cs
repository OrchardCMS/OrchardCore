using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageFacebookApp = new("ManageFacebookApp", LocalizedString.Create("View and edit the Facebook app.", typeof(Permissions)));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageFacebookApp,
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
