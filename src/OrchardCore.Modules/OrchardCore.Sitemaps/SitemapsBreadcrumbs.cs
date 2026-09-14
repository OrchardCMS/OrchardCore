namespace OrchardCore.Sitemaps;

/// <summary>
/// The names of the breadcrumbs rendered by the sitemaps screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class SitemapsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the sitemaps list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Sitemaps";

    /// <summary>
    /// The breadcrumb of the sitemap creation screen.
    /// </summary>
    public const string Create = "SitemapsCreate";

    /// <summary>
    /// The breadcrumb of the sitemap edition screen.
    /// </summary>
    public const string Edit = "SitemapsEdit";

    /// <summary>
    /// The breadcrumb of the sitemap display screen.
    /// </summary>
    public const string Display = "SitemapsDisplay";

    /// <summary>
    /// The breadcrumb of the sitemap source creation screen.
    /// </summary>
    public const string SourceCreate = "SitemapsSourceCreate";

    /// <summary>
    /// The breadcrumb of the sitemap source edition screen.
    /// </summary>
    public const string SourceEdit = "SitemapsSourceEdit";

    /// <summary>
    /// The breadcrumb of the sitemap indexes list. It is named after the list of that screen.
    /// </summary>
    public const string IndexesList = "SitemapIndexes";

    /// <summary>
    /// The breadcrumb of the sitemap index creation screen.
    /// </summary>
    public const string IndexesCreate = "SitemapIndexesCreate";

    /// <summary>
    /// The breadcrumb of the sitemap index edition screen.
    /// </summary>
    public const string IndexesEdit = "SitemapIndexesEdit";

    /// <summary>
    /// The breadcrumb of the sitemaps cache list. It is named after the list of that screen.
    /// </summary>
    public const string Cache = "SitemapCache";

    /// <summary>
    /// The key under which a source screen passes the identifier of the sitemap it belongs to.
    /// </summary>
    public const string SitemapIdKey = "SitemapId";
}
