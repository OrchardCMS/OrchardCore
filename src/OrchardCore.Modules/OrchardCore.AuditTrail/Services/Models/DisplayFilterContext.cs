using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayFilterContext
    {
        public IShapeFactory ShapeFactory { get; set; }
        public Filters Filters { get; set; }
        public Shape FilterDisplay { get; set; }


        public DisplayFilterContext(IShapeFactory shapeFactory, Filters filters, Shape filterDisplay)
        {
            ShapeFactory = shapeFactory;
            Filters = filters;
            FilterDisplay = filterDisplay;
        }
    }
}
