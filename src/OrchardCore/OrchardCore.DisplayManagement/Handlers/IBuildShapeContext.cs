using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Handlers
{
    /// <summary>
    /// Represents a context object that is used to build and place a list of shape representing
    /// something that has to be displayed.
    /// </summary>
    public interface IBuildShapeContext
    {
        IShape Shape { get; }
        IShapeFactory ShapeFactory { get; }
        dynamic New { get; }
        IZoneHolding Layout { get; set; }
        string GroupId { get; }
        FindPlacementDelegate FindPlacement { get; set; }
    }
}
