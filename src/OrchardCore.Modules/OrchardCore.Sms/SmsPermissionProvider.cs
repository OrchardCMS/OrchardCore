using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sms;

public class SmsPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageSmsSettings = SmsPermissions.ManageSmsSettings;

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSmsSettings
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
