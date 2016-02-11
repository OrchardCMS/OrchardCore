using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.DisplayManagement.Handlers
{
    public class BuildDisplayContext : BuildShapeContext
    {
        public BuildDisplayContext(IShape model, string displayType, string groupId, IShapeFactory shapeFactory, IShape layout, IUpdateModel updater)
            : base(model, groupId, shapeFactory, layout, updater)
        {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }
    }
}
