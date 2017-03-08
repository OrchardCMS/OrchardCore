using Orchard.ContentManagement.Metadata.Builders;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentTypes.Editors
{
    public class UpdatePartEditorContext : UpdateContentDefinitionEditorContext<ContentPartDefinitionBuilder>
    {
        public UpdatePartEditorContext(
                ContentPartDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }
}
