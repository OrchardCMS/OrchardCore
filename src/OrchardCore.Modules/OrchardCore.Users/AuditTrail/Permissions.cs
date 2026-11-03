using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users.AuditTrail;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewUserAuditTrailEvents = new(
        nameof(ViewUserAuditTrailEvents),
        LocalizationSource.Create<Permissions>("View Audit Trail events about users"),
        [],
        isSecurityCritical: true);

    public static readonly Permission ViewOwnUserAuditTrailEvents = new(
        nameof(ViewOwnUserAuditTrailEvents),
        LocalizationSource.Create<Permissions>("View Audit Trail events about own user"),
        [ViewUserAuditTrailEvents],
        isSecurityCritical: true);
    
    public static readonly Permission ManageUserAuditTrailSettings = new(
        nameof(ManageUserAuditTrailSettings),
        LocalizationSource.Create<Permissions>("Manage Audit Trail settings for user events"),
        [],
        isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewUserAuditTrailEvents,
        ViewOwnUserAuditTrailEvents,
        ManageUserAuditTrailSettings,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ViewOwnUserAuditTrailEvents,
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
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Authenticated,
            Permissions = _generalPermissions,
        },
    ];
}

