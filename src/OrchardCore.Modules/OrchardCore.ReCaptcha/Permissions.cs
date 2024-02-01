using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReCaptcha;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageReCaptchaSettings = new("ManageReCaptchaSettings", "Manage ReCaptcha Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageReCaptchaSettings,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
