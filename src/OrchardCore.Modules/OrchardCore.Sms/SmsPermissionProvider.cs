using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms;

public sealed class SmsPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageSmsSettings = SmsPermissions.ManageSmsSettings;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSmsSettings,
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
