using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailEventSettingsViewModel
    {
        public string Event { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsMandatory { get; set; }
    }
}
