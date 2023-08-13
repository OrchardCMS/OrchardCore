using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Seo;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

public class SitemapsRobotsProvider : IRobotsProvider
{
    private readonly ISitemapManager _sitemapManager;
    private readonly ISiteService _siteService;

    public SitemapsRobotsProvider(
        ISitemapManager sitemapManager,
        ISiteService siteService)
    {
        _sitemapManager = sitemapManager;
        _siteService = siteService;
    }

    public async Task<string> GetContentAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();

        if (String.IsNullOrEmpty(site.BaseUrl))
        {
            // Can't create sitemap links if there is not set base-url.

            return null;
        }

        var settings = site.As<SitemapsRobotsSettings>();

        if (!settings.IncludeSitemaps)
        {
            return null;
        }

        var baseUrl = site.BaseUrl.TrimEnd('/');

        var sitemaps = await _sitemapManager.GetSitemapsAsync();

        var content = new StringBuilder();
        var paths = new HashSet<string>();
        var sitemapIds = new HashSet<string>();

        // First we add sitemap indexes since they contain other sitemaps.
        // We don't have to add a sitemap, if it is already included in an enabled index.
        foreach (var sitemap in sitemaps.OfType<SitemapIndex>())
        {
            if (!sitemap.Enabled)
            {
                continue;
            }

            if (paths.Add(sitemap.Path))
            {
                content.AppendLine(GetSitemapEntry(baseUrl, sitemap.Path));

                sitemapIds.Add(sitemap.SitemapId);

                foreach (var source in sitemap.SitemapSources.Cast<SitemapIndexSource>())
                {
                    foreach (var containedSitemapIds in source.ContainedSitemapIds)
                    {
                        sitemapIds.Add(containedSitemapIds);
                    }
                }
            }
        }

        // Add any sitemaps that do not belong to an enabled index.
        foreach (var sitemap in sitemaps.OfType<Sitemap>())
        {
            if (!sitemap.Enabled || !sitemapIds.Add(sitemap.SitemapId))
            {
                continue;
            }

            if (paths.Add(sitemap.Path))
            {
                content.AppendLine(GetSitemapEntry(baseUrl, sitemap.Path));
            }
        }

        return content.ToString();
    }

    private static string GetSitemapEntry(string baseUrl, string path)
    {
        return $"Sitemap: {baseUrl}/{path}";
    }
}
