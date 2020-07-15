using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayAdditionalColumnsContext
    {
        public AuditTrailEvent AuditTrailEvent { get; }
        public Shape Display { get; }


        public DisplayAdditionalColumnsContext(AuditTrailEvent auditTrailEvent, Shape display)
        {
            AuditTrailEvent = auditTrailEvent;
            Display = display;
        }
    }
}
