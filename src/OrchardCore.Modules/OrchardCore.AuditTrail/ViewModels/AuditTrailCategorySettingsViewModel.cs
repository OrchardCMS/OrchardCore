using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailCategorySettingsViewModel
    {
        public string Name { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public IList<AuditTrailEventSettingsViewModel> Events { get; set; } = new List<AuditTrailEventSettingsViewModel>();
    }
}
