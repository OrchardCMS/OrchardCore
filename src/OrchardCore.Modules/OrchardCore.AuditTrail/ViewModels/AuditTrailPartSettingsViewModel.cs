using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AuditTrail.Settings;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailPartSettingsViewModel
    {
        public bool ShowCommentInput { get; set; }

        [BindNever]
        public AuditTrailPartSettings Settings { get; set; }
    }
}
