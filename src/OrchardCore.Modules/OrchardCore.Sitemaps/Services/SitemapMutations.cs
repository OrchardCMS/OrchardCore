using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

internal static class SitemapMutations
{
    public static bool Apply(SitemapType sitemap, string name, string path, bool enabled)
    {
        path = path.TrimStart('/');
        var changed = sitemap.Name != name || sitemap.Path != path || sitemap.Enabled != enabled;
        sitemap.Name = name;
        sitemap.Path = path;
        sitemap.Enabled = enabled;
        return changed;
    }

    public static bool SetEnabled(SitemapType sitemap, bool enabled)
    {
        if (sitemap.Enabled == enabled) { return false; }
        sitemap.Enabled = enabled;
        return true;
    }
}
