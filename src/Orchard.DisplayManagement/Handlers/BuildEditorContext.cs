namespace Orchard.DisplayManagement.Handlers
{
    public class BuildEditorContext : BuildShapeContext
    {
        public BuildEditorContext(IShape model, string groupId, IShapeFactory shapeFactory, IShape layout)
            : base(model, groupId, shapeFactory, layout)
        {
        }
    }
}
