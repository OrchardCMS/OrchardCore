using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.DisplayManagement.Handlers
{
    public class UpdateEditorContext : BuildEditorContext
    {
        public UpdateEditorContext(IShape model, string groupId, string htmlFieldPrefix, IShapeFactory shapeFactory, IShape layout, IUpdateModel updater)
            : base(model, groupId, htmlFieldPrefix, shapeFactory, layout, updater)
        {
        }
    }
}
