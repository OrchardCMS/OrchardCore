using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageMicrosoftAuthentication
        = new("ManageMicrosoftAuthentication", LocalizedString.Create("Manage Microsoft Authentication settings", typeof(Permissions)));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageMicrosoftAuthentication,
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
