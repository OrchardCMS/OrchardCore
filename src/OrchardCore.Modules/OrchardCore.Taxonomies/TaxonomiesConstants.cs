namespace OrchardCore.Taxonomies;

/// <summary>
/// The names of the breadcrumbs rendered by the taxonomy term screens, and the keys of the contextual data each of
/// them carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class TaxonomiesConstants
{
    /// <summary>
    /// The breadcrumb of the term creation screen. It carries <see cref="TermTypeDisplayNameKey"/> and
    /// <see cref="TaxonomyContentItemIdKey"/>.
    /// </summary>
    public const string Create = "TaxonomiesCreate";

    /// <summary>
    /// The breadcrumb of the term edition screen. It carries <see cref="TermTypeDisplayNameKey"/> and
    /// <see cref="TaxonomyContentItemIdKey"/>.
    /// </summary>
    public const string Edit = "TaxonomiesEdit";

    /// <summary>
    /// The key under which a screen passes the display name of the term type it is about.
    /// </summary>
    public const string TermTypeDisplayNameKey = "TermTypeDisplayName";

    /// <summary>
    /// The key under which a screen passes the identifier of the taxonomy the term belongs to.
    /// </summary>
    public const string TaxonomyContentItemIdKey = "TaxonomyContentItemId";
}
