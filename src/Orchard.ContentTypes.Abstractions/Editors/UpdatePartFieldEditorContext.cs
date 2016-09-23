using Orchard.ContentManagement.Metadata.Builders;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentTypes.Editors
{
    public class UpdatePartFieldEditorContext : UpdateContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
    {
        public UpdatePartFieldEditorContext(
                ContentPartFieldDefinitionBuilder builder,
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
