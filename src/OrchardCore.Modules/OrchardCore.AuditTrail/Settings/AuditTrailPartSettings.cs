using System.ComponentModel;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailPartSettings
    {
        [DefaultValue(true)]
        public bool ShowCommentInput { get; set; } = true;
    }
}
