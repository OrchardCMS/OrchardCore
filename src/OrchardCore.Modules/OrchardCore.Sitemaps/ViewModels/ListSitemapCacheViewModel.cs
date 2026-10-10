using OrchardCore.DisplayManagement;

namespace OrchardCore.Sitemaps.ViewModels;

public class ListSitemapCacheViewModel
{
    public string[] CachedFileNames { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the cached files in the configured layout.
    /// </summary>
    public IShape List { get; set; }
}
