using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentTypes.Editors
{
    public class UpdatePartFieldEditorContext : UpdateContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
    {
        public UpdatePartFieldEditorContext(
                ContentPartFieldDefinitionBuilder builder,
                IShape model,
                string groupId,
                bool isNew,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, isNew, shapeFactory, layout, updater)
        {
        }
    }
}
