using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.ContentTypes.Editors;

public class UpdatePartFieldEditorContext : UpdateContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
{
    public UpdatePartFieldEditorContext(
            ContentPartFieldDefinitionBuilder builder,
            IShape model,
            string groupId,
            bool isNew,
            IShapeFactory shapeFactory,
            IZoneHolding layout,
            IUpdateModel updater,
            HttpContext httpContext) : base(builder, model, groupId, isNew, shapeFactory, layout, updater, httpContext)
    {
    }
}
