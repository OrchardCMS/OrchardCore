using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AuditTrail;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewAuditTrail = AuditTrailPermissions.ViewAuditTrail;
    public static readonly Permission ManageAuditTrailSettings = AuditTrailPermissions.ManageAuditTrailSettings;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewAuditTrail,
        ManageAuditTrailSettings,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions
        },
    ];
}
