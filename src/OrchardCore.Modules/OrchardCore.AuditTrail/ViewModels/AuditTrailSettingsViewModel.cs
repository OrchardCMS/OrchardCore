using System.Collections.Generic;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailSettingsViewModel
    {
        public bool EnableClientIpAddressLogging { get; set; }
        public List<AuditTrailCategorySettingsViewModel> Categories { get; set; }
        public List<IgnoredContentTypesViewModel> IgnoredContentTypes { get; set; }

        public AuditTrailSettingsViewModel()
        {
            IgnoredContentTypes = new List<IgnoredContentTypesViewModel>();
            Categories = new List<AuditTrailCategorySettingsViewModel>();
        }
    }
}
