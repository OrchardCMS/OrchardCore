using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayEventContext
    {
        public DisplayEventContext(IShape shape)
        {
            Shape = shape;
        }

        public IShape Shape { get; }
    }
}
