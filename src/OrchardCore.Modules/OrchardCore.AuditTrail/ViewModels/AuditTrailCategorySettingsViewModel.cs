using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailCategorySettingsViewModel
    {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public List<AuditTrailEventSettingsViewModel> Events { get; set; }
    }
}