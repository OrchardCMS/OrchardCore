using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailEventSummaryViewModel
    {
        public AuditTrailEvent AuditTrailEvent { get; set; }
        public AuditTrailEventDescriptor EventDescriptor { get; set; }
        public AuditTrailCategoryDescriptor CategoryDescriptor { get; set; }
        public IShape SummaryShape { get; set; }
        public IShape ActionsShape { get; set; }
    }
}
