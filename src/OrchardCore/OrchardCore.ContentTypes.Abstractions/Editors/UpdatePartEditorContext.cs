using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.ContentTypes.Editors
{
    public class UpdatePartEditorContext : UpdateContentDefinitionEditorContext<ContentPartDefinitionBuilder>
    {
        public UpdatePartEditorContext(
                ContentPartDefinitionBuilder builder,
                IShape model,
                string groupId,
                bool isNew,
                IShapeFactory shapeFactory,
                IZoneHolding layout,
                IUpdateModel updater)
            : base(builder, model, groupId, isNew, shapeFactory, layout, updater)
        {
        }
    }
}
