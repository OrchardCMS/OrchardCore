using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public class PixelPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageFacebookPixelPermission = FacebookConstants.ManageFacebookPixelPermission;

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageFacebookPixelPermission,
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
