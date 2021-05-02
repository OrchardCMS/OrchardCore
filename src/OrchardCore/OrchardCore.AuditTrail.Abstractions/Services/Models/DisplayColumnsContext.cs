using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayColumnsContext
    {
        public AuditTrailEvent AuditTrailEvent { get; }
        public IShape ColumnsShape { get; }

        public DisplayColumnsContext(AuditTrailEvent auditTrailEvent, IShape shape)
        {
            AuditTrailEvent = auditTrailEvent;
            ColumnsShape = shape;
        }
    }
}
