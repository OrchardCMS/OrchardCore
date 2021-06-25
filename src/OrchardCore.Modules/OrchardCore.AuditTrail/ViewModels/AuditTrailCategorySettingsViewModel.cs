using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailCategorySettingsViewModel
    {
        public string Name { get; set; }
        
        public AuditTrailEventSettingsViewModel[] Events { get; set; }

        [BindNever]
        public LocalizedString LocalizedName { get; set; }
    }
}
