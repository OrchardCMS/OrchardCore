using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Handlers;

public class BuildDisplayContext : BuildShapeContext
{
    public BuildDisplayContext(
        IShape shape,
        string displayType,
        string groupId,
        IShapeFactory shapeFactory,
        IZoneHolding layout,
        IUpdateModel updater,
        HttpContext httpContext) : base(shape, groupId, shapeFactory, layout, updater, httpContext)
    {
        DisplayType = displayType;
    }

    public string DisplayType { get; private set; }
}
