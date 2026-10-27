using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Maps a shape template file to the name of the template overriding it, the way <see cref="BasicShapeTemplateHarvester"/>
/// maps it to a shape type, but keeping the case of the file name so the name reads like the documented ones.
/// </summary>
public static class SourceTemplateNames
{
    private static readonly HashSet<string> s_subPaths = new(new BasicShapeTemplateHarvester().SubPaths(), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the template name of a file.
    /// </summary>
    /// <param name="relativePath">The path of the file inside its extension, e.g. <c>Views/Content-Article.Summary.liquid</c>.</param>
    /// <param name="templateName">The template name, e.g. <c>Content_Summary__Article</c>.</param>
    /// <returns>Whether the file is a shape template.</returns>
    public static bool TryGetTemplateName(string relativePath, out string templateName)
    {
        templateName = null;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        relativePath = relativePath.Replace('\\', '/').Trim('/');

        var subPath = Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? string.Empty;
        if (!s_subPaths.Contains(subPath))
        {
            return false;
        }

        var fileName = Path.GetFileNameWithoutExtension(relativePath);

        // Files such as '_ViewImports' or '_ViewStart' are not shape templates.
        if (string.IsNullOrEmpty(fileName) || fileName.StartsWith('_') || string.IsNullOrEmpty(Path.GetExtension(relativePath)))
        {
            return false;
        }

        var lastDash = fileName.LastIndexOf('-');
        var lastDot = fileName.LastIndexOf('.');

        templateName = lastDot <= 0 || lastDot < lastDash
            ? Adjust(subPath, fileName, null)
            : Adjust(subPath, fileName[..lastDot], fileName[(lastDot + 1)..]);

        return true;
    }

    private static string Adjust(string subPath, string fileName, string displayType)
    {
        var leader = string.Empty;
        if (subPath.StartsWith("Views/", StringComparison.Ordinal) && !string.Equals(subPath, "Views/Items", StringComparison.OrdinalIgnoreCase))
        {
            leader = string.Concat(subPath.AsSpan("Views/".Length), "_");
        }

        var shapeType = leader + fileName.Replace("--", "__", StringComparison.Ordinal).Replace("-", "__", StringComparison.Ordinal).Replace('.', '_');

        if (string.IsNullOrEmpty(displayType))
        {
            return shapeType;
        }

        var firstBreakingSeparator = shapeType.IndexOf("__", StringComparison.Ordinal);
        if (firstBreakingSeparator <= 0)
        {
            return shapeType + "_" + displayType;
        }

        return string.Concat(shapeType.AsSpan(0, firstBreakingSeparator), "_", displayType, shapeType.AsSpan(firstBreakingSeparator));
    }
}
