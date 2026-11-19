namespace OrchardCore.Templates.Models;

/// <summary>
/// A part of a template source that can be listed on its own, e.g. one of the themes of the theme templates.
/// </summary>
public sealed class TemplateSourceScope
{
    /// <summary>
    /// Gets or sets the technical name of the scope, used in the URL of the list, e.g. a theme identifier.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the scope shown to the user, e.g. the name of a theme and whether it is active.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets whether the templates of the scope are in use, e.g. the active theme, so that they can be cloned.
    /// </summary>
    public bool IsInUse { get; set; }
}
