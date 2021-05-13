using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayEventContext
    {
        public DisplayEventContext(IShape shape)
        {
            EventShape = shape;
        }

        public IShape EventShape { get; }
    }
}
