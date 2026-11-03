using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AuditTrail;

public static class AuditTrailPermissions
{
    public static readonly Permission ViewAuditTrail =
        new(nameof(ViewAuditTrail), LocalizationSource.Create("View audit trail", typeof(AuditTrailPermissions)));

    public static readonly Permission ManageAuditTrailSettings =
        new(nameof(ManageAuditTrailSettings), LocalizationSource.Create("Manage audit trail settings", typeof(AuditTrailPermissions)));
}
