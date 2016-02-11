using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.DisplayManagement.Handlers
{
    public class BuildEditorContext : BuildShapeContext
    {
        public BuildEditorContext(IShape model, string groupId, IShapeFactory shapeFactory, IShape layout, IUpdateModel updater)
            : base(model, groupId, shapeFactory, layout, updater)
        {
        }
    }
}
