using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AuditTrail.Settings;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailPartSettingsViewModel
    {
        public bool ShowAuditTrailCommentInput { get; set; }

        [BindNever]
        public AuditTrailPartSettings AuditTrailPartSettings { get; set; }
    }
}
