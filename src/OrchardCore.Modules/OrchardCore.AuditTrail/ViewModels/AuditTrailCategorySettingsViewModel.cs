using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailCategorySettingsViewModel
    {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public AuditTrailEventSettingsViewModel[] Events { get; set; }
    }
}
