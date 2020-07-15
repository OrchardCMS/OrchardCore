using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailEventSummaryViewModel
    {
        public AuditTrailEvent AuditTrailEvent { get; set; }
        public dynamic AdditionalColumnsShapes { get; set; }
        public AuditTrailEventDescriptor EventDescriptor { get; set; }
        public AuditTrailCategoryDescriptor CategoryDescriptor { get; set; }
        public dynamic SummaryShape { get; set; }
        public dynamic ActionsShape { get; set; }
    }
}
