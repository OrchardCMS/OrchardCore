using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentTypes.Editors
{
    public class UpdateContentDefinitionEditorContext<TBuilder> : UpdateEditorContext
    {
        public UpdateContentDefinitionEditorContext(
            TBuilder builder,
            IShape model,
            string groupId,
            bool isNew,
            IShapeFactory shapeFactory,
            IShape layout,
            IUpdateModel updater)
            : base(model, groupId, isNew, "", shapeFactory, layout, updater)
        {
            Builder = builder;
        }

        public TBuilder Builder { get; private set; }
    }
}
