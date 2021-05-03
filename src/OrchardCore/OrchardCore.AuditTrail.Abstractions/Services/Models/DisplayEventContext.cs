using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayEventContext
    {
        public IShape EventShape { get; }

        public DisplayEventContext(IShape shape)
        {
            EventShape = shape;
        }
    }
}
