using OrchardCore.Security.Permissions;

namespace OrchardCore.Email.Core;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", "Manage Email Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageEmailSettings,
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
