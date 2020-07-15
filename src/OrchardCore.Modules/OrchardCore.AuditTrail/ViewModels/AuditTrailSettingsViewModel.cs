using System.Collections.Generic;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailSettingsViewModel
    {
        public bool EnableClientIpAddressLogging { get; set; }
        public List<AuditTrailCategorySettingsViewModel> Categories { get; set; }
        public List<BlacklistedContentTypesViewModel> BlacklistedContentTypes { get; set; }


        public AuditTrailSettingsViewModel()
        {
            BlacklistedContentTypes = new List<BlacklistedContentTypesViewModel>();
            Categories = new List<AuditTrailCategorySettingsViewModel>();
        }
    }
}