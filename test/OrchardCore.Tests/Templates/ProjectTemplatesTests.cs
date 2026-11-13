using System.Text.RegularExpressions;

namespace OrchardCore.Tests.Templates;

/// <summary>
/// Validates the content of the project templates, which every generated project inherits.
/// </summary>
public partial class ProjectTemplatesTests
{
    private static readonly string[] s_viewExtensions = [".cshtml", ".html", ".liquid"];

    [Fact]
    public void ProjectTemplates_ExternalResources_HaveSubresourceIntegrity()
    {
        var templatesPath = Path.Combine(GetRepositoryRoot(), "src", "Templates");

        var views = Directory.EnumerateFiles(templatesPath, "*", SearchOption.AllDirectories)
            .Where(path => s_viewExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(views);

        foreach (var path in views)
        {
            foreach (var tag in ExternalResourceRegex().Matches(File.ReadAllText(path)).Select(match => match.Value))
            {
                var relativePath = Path.GetRelativePath(templatesPath, path);

                Assert.True(tag.Contains("integrity=\"sha", StringComparison.Ordinal), $"'{relativePath}' loads an external resource without an 'integrity' attribute: {tag}");
                Assert.True(tag.Contains("crossorigin=\"anonymous\"", StringComparison.Ordinal), $"'{relativePath}' loads an external resource without a 'crossorigin=\"anonymous\"' attribute: {tag}");
            }
        }
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            Assert.Skip("The repository sources are not available.");
        }

        return directory.FullName;
    }

    // Matches the 'script' and 'link' tags referencing an absolute or protocol-relative URL.
    [GeneratedRegex("""<(?:script|link)\b[^>]*\b(?:src|href)\s*=\s*["'](?:https?:)?//[^>]*>""", RegexOptions.IgnoreCase)]
    private static partial Regex ExternalResourceRegex();
}
