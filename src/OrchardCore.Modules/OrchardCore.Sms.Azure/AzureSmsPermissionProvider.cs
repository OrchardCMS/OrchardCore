using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms.Azure;

public class AzureSmsPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageSmsSettings = AzureSmsPermissions.ManageAzureSmsSettings;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSmsSettings,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync() => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
