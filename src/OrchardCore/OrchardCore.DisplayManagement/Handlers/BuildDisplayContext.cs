using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.DisplayManagement.Handlers
{
    public class BuildDisplayContext : BuildShapeContext
    {
        public BuildDisplayContext(IShape shape, string displayType, string groupId, IShapeFactory shapeFactory, IShape layout, IUpdateModel updater)
            : base(shape, groupId, shapeFactory, layout, updater)
        {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }
    }
}
