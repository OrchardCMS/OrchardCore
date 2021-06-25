using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailEventSettingsViewModel
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsMandatory { get; set; }

        [BindNever]
        public LocalizedString LocalizedName { get; set; }

        [BindNever]
        public LocalizedString Description { get; set; }
    }
}
