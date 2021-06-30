using OrchardCore.Security.Permissions;

namespace OrchardCore.AuditTrail
{
    public class AuditTrailPermissions
    {
        public static readonly Permission ViewAuditTrail = new Permission(nameof(ViewAuditTrail), "View audit trail");
        public static readonly Permission ManageAuditTrailSettings = new Permission(nameof(ManageAuditTrailSettings), "Manage audit trail settings");
    }
}
