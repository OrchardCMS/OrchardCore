using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailCategorySettingsViewModel
    {
        public string Name { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public AuditTrailEventSettingsViewModel[] Events { get; set; }
    }
}
