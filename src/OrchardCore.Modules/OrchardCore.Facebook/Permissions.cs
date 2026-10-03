using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageFacebookApp = new("ManageFacebookApp", new LocalizationSource("View and edit the Facebook app.", typeof(Permissions)));

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
