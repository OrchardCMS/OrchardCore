namespace Orchard.DisplayManagement.Handlers
{
    public class BuildDisplayContext : BuildShapeContext
    {
        public BuildDisplayContext(IShape model, string displayType, string groupId, IShapeFactory shapeFactory, IShape layout)
            : base(model, groupId, shapeFactory, layout)
        {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }
    }
}
