using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public class GoogleAnalyticsPermissionsProvider : IPermissionProvider
{
    public static readonly Permission ManageGoogleAnalytics = Permissions.ManageGoogleAnalytics;

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageGoogleAnalytics,
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
