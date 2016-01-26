using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.DisplayManagement.Handlers
{
    public class UpdateEditorContext : BuildEditorContext
    {
        public UpdateEditorContext(IShape model, string groupId, IShapeFactory shapeFactory, IShape layout, IUpdateModel updateModel)
            : base(model, groupId, shapeFactory, layout)
        {
            Updater = updateModel;
        }

        public IUpdateModel Updater { get; }
    }
}
