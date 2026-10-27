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

    /// <summary>
    /// Guesses whether a template renders the admin, e.g. an editor, rather than the front end, from the words of its
    /// name, e.g. <c>TextField_Edit</c>, <c>Content_SummaryAdmin</c> or <c>ContentPartSettings_Edit</c>. Modules don't
    /// tell which templates render the admin, so the guess is only used to filter them.
    /// </summary>
    public static bool IsAdminTemplate(string templateName)
    {
        if (string.IsNullOrEmpty(templateName))
        {
            return false;
        }

        foreach (var word in GetWords(templateName))
        {
            if (s_adminWords.Contains(word))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly HashSet<string> s_adminWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Display types and shapes of editors.
        "Admin", "Edit", "Editor", "Thumbnail", "Settings", "Option", "Options", "Picker", "Wizard", "Modal",

        // Admin pages: the dashboard, the admin menu, the lists and the designers of workflows, deployment
        // plans, data pipelines, rules, recipes, tenants and roles.
        "Dashboard", "Navigation", "Tree", "Filter", "Filters", "Manage", "Design", "Workflow", "Workflows", "Deployment",
        "Pipeline", "Condition", "Conditions", "Recipe", "Tenant", "Tenants", "Permissions", "Roles",
    };

    // The words of a name, split on underscores and before upper case letters, e.g. 'SummaryAdmin' is 'Summary' and 'Admin'.
    private static IEnumerable<string> GetWords(string name)
    {
        var start = -1;

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (c == '_' || c == '-' || c == '.')
            {
                if (start >= 0)
                {
                    yield return name[start..i];
                }

                start = -1;
            }
            else if (char.IsUpper(c) && start >= 0 && !char.IsUpper(name[i - 1]))
            {
                yield return name[start..i];
                start = i;
            }
            else if (start < 0)
            {
                start = i;
            }
        }

        if (start >= 0)
        {
            yield return name[start..];
        }
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
