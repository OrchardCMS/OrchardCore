namespace OrchardCore.Sitemaps;

/// <summary>
/// The admin list of sitemaps rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="SitemapsAdminListColumnDriver"/>.
/// </summary>
public static class SitemapsAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Sitemaps</c> and <c>AdminListCell__Sitemaps__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Sitemaps";
}

/// <summary>
/// The admin list of sitemap indexes rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="SitemapIndexesAdminListColumnDriver"/>.
/// </summary>
public static class SitemapIndexesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__SitemapIndexes</c> and <c>AdminListCell__SitemapIndexes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "SitemapIndexes";
}

/// <summary>
/// The admin list of cached sitemap files rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="SitemapCacheAdminListColumnDriver"/>.
/// </summary>
public static class SitemapCacheAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__SitemapCache</c> and <c>AdminListCell__SitemapCache__{Column}</c> alternates.
    /// </summary>
    public const string Name = "SitemapCache";
}
