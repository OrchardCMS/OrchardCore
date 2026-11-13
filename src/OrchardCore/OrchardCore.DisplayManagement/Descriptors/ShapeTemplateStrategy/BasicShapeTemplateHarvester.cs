namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;

public class BasicShapeTemplateHarvester : IShapeTemplateHarvester
{
    public IEnumerable<string> SubPaths()
    {
        return new[] { "Views", "Views/Items", "Views/Parts", "Views/Fields", "Views/Elements" };
    }

    public IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info)
    {
        var shapeType = GetShapeType(info.SubPath, info.FileName, out var displayType);

        yield return new HarvestShapeHit
        {
            ShapeType = shapeType.ToLowerInvariant(),
            DisplayType = displayType,
        };
    }

    /// <summary>
    /// Gets the type of the shape a template file renders, keeping the case of the file name.
    /// </summary>
    /// <param name="subPath">The folder of the file inside its extension, e.g. <c>Views/Parts</c>.</param>
    /// <param name="fileName">The name of the file without its extension, e.g. <c>Content-Article.Summary</c>.</param>
    /// <param name="displayType">The display type of the file name, e.g. <c>Summary</c>, if any.</param>
    /// <returns>The shape type, e.g. <c>Content_Summary__Article</c>.</returns>
    public static string GetShapeType(string subPath, string fileName, out string displayType)
    {
        var lastDash = fileName.LastIndexOf('-');
        var lastDot = fileName.LastIndexOf('.');
        if (lastDot <= 0 || lastDot < lastDash)
        {
            displayType = null;

            return Adjust(subPath, fileName, null);
        }

        displayType = fileName[(lastDot + 1)..];

        return Adjust(subPath, fileName[..lastDot], displayType);
    }

    private static string Adjust(string subPath, string fileName, string displayType)
    {
        var leader = "";
        if (subPath.StartsWith("Views/", StringComparison.Ordinal) && subPath != "Views/Items")
        {
            leader = string.Concat(subPath.AsSpan("Views/".Length), "_");
        }

        // canonical shape type names must not have - or . to be compatible
        // with display and shape api calls)))
        var shapeType = leader + fileName.Replace("--", "__").Replace("-", "__").Replace('.', '_');

        if (string.IsNullOrEmpty(displayType))
        {
            return shapeType;
        }

        var firstBreakingSeparator = shapeType.IndexOf("__", StringComparison.Ordinal);
        if (firstBreakingSeparator <= 0)
        {
            return shapeType + "_" + displayType;
        }

        return string.Concat(
            shapeType.AsSpan(0, firstBreakingSeparator), "_", displayType, shapeType.AsSpan(firstBreakingSeparator));
    }
}
