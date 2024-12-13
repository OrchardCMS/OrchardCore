using OrchardCore.Security.Permissions;

namespace OrchardCore.AuditTrail;

public sealed class Permissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.AuditTrail.AuditTrailPermissions.ViewAuditTrail'.")]
    public static readonly Permission ViewAuditTrail = AuditTrailPermissions.ViewAuditTrail;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.AuditTrail.AuditTrailPermissions.ManageAuditTrailSettings'.")]
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
