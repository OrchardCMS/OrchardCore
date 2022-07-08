using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Handlers
{
    public class BuildEditorContext : BuildShapeContext
    {
        public bool IsNew { get; set; }

        public BuildEditorContext(IShape shape, string groupId, bool isNew, string htmlFieldPrefix, IShapeFactory shapeFactory, IZoneHolding layout, IUpdateModel updater)
            : base(shape, groupId, shapeFactory, layout, updater)
        {
            HtmlFieldPrefix = htmlFieldPrefix;
            IsNew = isNew;
        }
    }
}
