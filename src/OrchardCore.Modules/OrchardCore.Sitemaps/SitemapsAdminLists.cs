namespace OrchardCore.Sitemaps;

/// <summary>
/// The admin list of sitemaps rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="SitemapsAdminListColumnProvider"/>.
/// </summary>
public static class SitemapsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Sitemaps</c> and <c>AdminListCell__Sitemaps__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Sitemaps";
}

/// <summary>
/// The admin list of sitemap indexes rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="SitemapIndexesAdminListColumnProvider"/>.
/// </summary>
public static class SitemapIndexesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__SitemapIndexes</c> and <c>AdminListCell__SitemapIndexes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "SitemapIndexes";
}

/// <summary>
/// The admin list of cached sitemap files rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="SitemapCacheAdminListColumnProvider"/>.
/// </summary>
public static class SitemapCacheAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__SitemapCache</c> and <c>AdminListCell__SitemapCache__{Column}</c> alternates.
    /// </summary>
    public const string Name = "SitemapCache";
}
