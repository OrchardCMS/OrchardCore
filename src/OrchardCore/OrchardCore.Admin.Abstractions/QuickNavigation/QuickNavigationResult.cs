namespace OrchardCore.Admin.QuickNavigation;

/// <summary>
/// A localized, authorized destination in the client-side quick navigation index.
/// All labels are plain text, not HTML.
/// </summary>
public sealed class QuickNavigationResult
{
    /// <summary>
    /// Gets or sets the originating source name, assigned by the index endpoint.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the source-specific identifier, assigned by the index endpoint.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the title localized for the current UI culture.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the localized breadcrumb, ordered from outermost to innermost.
    /// </summary>
    public string[] Path { get; set; } = [];

    /// <summary>
    /// Gets or sets the destination URL, including the tenant and admin prefixes.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// Gets or sets the optional link target.
    /// </summary>
    public string Target { get; set; }
}
