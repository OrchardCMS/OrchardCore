using OrchardCore.Security.Permissions;

namespace OrchardCore.Email.Core;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        EmailPermissions.ManageEmailSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'EmailPermissions.ManageEmailSettings'.")]
    public static readonly Permission ManageEmailSettings = EmailPermissions.ManageEmailSettings;

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
