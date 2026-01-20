using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageMicrosoftAuthentication
        = new("ManageMicrosoftAuthentication", "Manage Microsoft Authentication settings");

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
