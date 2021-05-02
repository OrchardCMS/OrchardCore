using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DisplayFiltersContext
    {
        public Filters Filters { get; set; }
        public AuditTrailCategoryDescriptor[] Categories { get; set; }
        public IShapeFactory ShapeFactory { get; set; }
        public IShape FiltersShape { get; set; }

        public DisplayFiltersContext(Filters filters, AuditTrailCategoryDescriptor[] categories, IShapeFactory shapeFactory, IShape shape)
        {
            Filters = filters;
            ShapeFactory = shapeFactory;
            FiltersShape = shape;
            Categories = categories;
        }
    }
}
