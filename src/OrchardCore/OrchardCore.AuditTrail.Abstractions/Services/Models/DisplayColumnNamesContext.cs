using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayColumnNamesContext
    {
        public IShape ColumnNamesShape { get; }

        public DisplayColumnNamesContext(IShape shape)
        {
            ColumnNamesShape = shape;
        }
    }
}
