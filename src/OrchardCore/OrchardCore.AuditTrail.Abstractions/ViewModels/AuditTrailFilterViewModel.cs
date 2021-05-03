using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailFilterViewModel
    {
        public string UserName { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Category { get; set; }
        public AuditTrailCategoryDescriptor[] Categories { get; set; }
    }
}
