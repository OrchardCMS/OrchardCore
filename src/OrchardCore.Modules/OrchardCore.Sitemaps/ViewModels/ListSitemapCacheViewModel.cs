using OrchardCore.Admin;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Sitemaps.ViewModels;

public class ListSitemapCacheViewModel
{
    public string[] CachedFileNames { get; set; }

    /// <summary>
    /// The data context for rendering the cached files in the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}
