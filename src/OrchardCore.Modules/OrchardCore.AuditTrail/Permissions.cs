using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AuditTrail;

public class Permissions : IPermissionProvider
{
    [Obsolete("This property will be removed in future release. Instead use 'AuditTrailPermissions.ViewAuditTrail'.")]
    public static readonly Permission ViewAuditTrail = AuditTrailPermissions.ViewAuditTrail;

    [Obsolete("This property will be removed in future release. Instead use 'AuditTrailPermissions.ManageAuditTrailSettings'.")]
    public static readonly Permission ManageAuditTrailSettings = AuditTrailPermissions.ManageAuditTrailSettings;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        AuditTrailPermissions.ViewAuditTrail,
        AuditTrailPermissions.ManageAuditTrailSettings,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions
        },
    ];
}
