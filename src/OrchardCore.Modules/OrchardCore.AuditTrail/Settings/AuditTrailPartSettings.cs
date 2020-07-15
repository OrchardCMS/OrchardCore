using System.ComponentModel;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailPartSettings
    {
        [DefaultValue(true)]
        public bool ShowAuditTrailCommentInput { get; set; } = true;
    }
}
