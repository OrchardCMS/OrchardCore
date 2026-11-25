using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.ContentTypes.Editors;

internal static class ShapeOptionsHelper
{
    /// <summary>
    /// Gets the names of the shapes rendering the options of an editor or display mode select, e.g.
    /// <c>TextField_Option</c> and <c>TextField_Option__Monaco</c> for the <c>TextField_Option</c> shape type.
    /// </summary>
    public static string[] GetOptionShapes(ShapeTable shapeTable, string shapeType)
    {
        return shapeTable.Bindings.Keys
            .Where(x => x.StartsWith(shapeType + "__", StringComparison.OrdinalIgnoreCase) || x.Equals(shapeType, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
