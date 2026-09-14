namespace OrchardCore.Indexing;

/// <summary>
/// The names of the breadcrumbs rendered by the indexes screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class IndexingBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the indexes list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Indexes";

    /// <summary>
    /// The breadcrumb of the index creation screen. It carries <see cref="DisplayNameKey"/>.
    /// </summary>
    public const string Create = "IndexesCreate";

    /// <summary>
    /// The breadcrumb of the index edition screen. It carries <see cref="DisplayNameKey"/>.
    /// </summary>
    public const string Edit = "IndexesEdit";

    /// <summary>
    /// The key under which a screen passes the display name of the index it is about.
    /// </summary>
    public const string DisplayNameKey = "DisplayName";
}
