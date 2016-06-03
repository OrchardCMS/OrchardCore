using Orchard.ContentManagement.MetaData.Builders;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentTypes.Editors
{
    public class UpdateTypePartEditorContext : UpdateContentDefinitionEditorContext<ContentTypePartDefinitionBuilder>
    {
        public UpdateTypePartEditorContext(
                ContentTypePartDefinitionBuilder builder,
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
