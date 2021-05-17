using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailEventSummaryViewModel
    {
        public AuditTrailEvent Event { get; set; }
        public AuditTrailEventDescriptor Descriptor { get; set; }
        public AuditTrailCategoryDescriptor Category { get; set; }
        public IShape SummaryShape { get; set; }
        public IShape ActionsShape { get; set; }
    }
}
