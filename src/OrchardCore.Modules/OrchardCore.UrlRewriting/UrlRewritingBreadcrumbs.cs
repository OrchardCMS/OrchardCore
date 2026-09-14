namespace OrchardCore.UrlRewriting;

/// <summary>
/// The names of the breadcrumbs rendered by the URL rewriting screens, and the keys of the contextual data each of
/// them carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class UrlRewritingBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the URL rewriting rules list. It is named after the list of that screen.
    /// </summary>
    public const string List = "UrlRewriting";

    /// <summary>
    /// The breadcrumb of the rule creation screen. It carries <see cref="DisplayNameKey"/>.
    /// </summary>
    public const string Create = "UrlRewritingCreate";

    /// <summary>
    /// The breadcrumb of the rule edition screen. It carries <see cref="DisplayNameKey"/>.
    /// </summary>
    public const string Edit = "UrlRewritingEdit";

    /// <summary>
    /// The key under which a screen passes the display name of the rule source it is about.
    /// </summary>
    public const string DisplayNameKey = "DisplayName";
}
