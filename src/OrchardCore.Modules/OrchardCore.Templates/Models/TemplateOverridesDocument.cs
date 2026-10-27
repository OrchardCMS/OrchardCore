using OrchardCore.Data.Documents;

namespace OrchardCore.Templates.Models;

/// <summary>
/// Records the templates that were cloned from a template source, so that their changes can be monitored.
/// </summary>
public class TemplateOverridesDocument : Document
{
    /// <summary>
    /// Gets the clone records, by template name.
    /// </summary>
    public Dictionary<string, TemplateOverride> Overrides { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Records the admin templates that were cloned from a template source.
/// </summary>
public sealed class AdminTemplateOverridesDocument : TemplateOverridesDocument
{
}

/// <summary>
/// Describes where a local template was cloned from.
/// </summary>
public sealed class TemplateOverride
{
    /// <summary>
    /// Gets or sets the name of the template source the template was cloned from, e.g. <c>Theme</c>.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the extension that shipped the template.
    /// </summary>
    public string ExtensionId { get; set; }

    /// <summary>
    /// Gets or sets the path of the cloned file inside its extension.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// Gets or sets the hash of the content that was cloned, compared to the local template to know if it was edited,
    /// and to the source file to know if it changed since.
    /// </summary>
    public string Hash { get; set; }

    /// <summary>
    /// Gets or sets when the template was cloned.
    /// </summary>
    public DateTime ClonedUtc { get; set; }
}
