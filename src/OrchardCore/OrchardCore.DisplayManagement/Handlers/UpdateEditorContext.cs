using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Handlers
{
    public class UpdateEditorContext : BuildEditorContext
    {

        public UpdateEditorContext(IShape model, string groupId, bool isNew, string htmlFieldPrefix, IShapeFactory shapeFactory,
            IZoneHolding layout, IUpdateModel updater)
            : base(model, groupId, isNew, htmlFieldPrefix, shapeFactory, layout, updater)
        {
        }

    }
}
