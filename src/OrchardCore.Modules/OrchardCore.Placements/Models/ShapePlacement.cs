namespace OrchardCore.Placements.Models;

/// <summary>
/// One entry of the placements admin list. Its name is the shape type of the rows built by
/// <see cref="Drivers.ShapePlacementDisplayDriver"/>, so themes override them with
/// <c>ShapePlacement-SummaryAdmin.cshtml</c>.
/// </summary>
public class ShapePlacement
{
    public string ShapeType { get; set; }
}
