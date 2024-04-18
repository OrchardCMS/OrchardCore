using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

[Feature(GoogleConstants.Features.GoogleAnalytics)]
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
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
