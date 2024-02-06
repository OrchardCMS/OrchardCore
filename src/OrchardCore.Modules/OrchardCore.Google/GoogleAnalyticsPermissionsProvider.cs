using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public class GoogleAnalyticsPermissionsProvider : IPermissionProvider
{
    public static readonly Permission ManageGoogleAnalytics = Permissions.ManageGoogleAnalytics;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageGoogleAnalytics,
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
