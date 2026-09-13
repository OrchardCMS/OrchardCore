using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.Security.Permissions;
using OrchardCore.Seo;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

internal sealed class SitemapsRobotsSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    public SitemapsRobotsSettingsSectionProvider(ISiteService siteService) { _siteService = siteService; }
    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "sitemaps-robots", DisplayName = "Sitemaps in robots.txt", FeatureId = "OrchardCore.Sitemaps",
        Description = "Controls the existing sitemap robots provider when both Sitemaps and SEO are enabled. A physical robots.txt still takes precedence.",
    };
    public Permission ReadPermission => SeoConstants.ManageSeoSettings;
    public Permission UpdatePermission => SeoConstants.ManageSeoSettings;
    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject { ["includeSitemaps"] = new JsonObject { ["type"] = "boolean" } },
    };
    public async Task<SiteSettingsSectionResponse> GetAsync() => Describe(await _siteService.GetSettingsAsync<SitemapsRobotsSettings>());
    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null) { return new() { Errors = new() { ["body"] = ["A settings object is required."] } }; }
        var errors = new Dictionary<string, string[]>();
        bool? include = null;
        foreach (var entry in values)
        {
            if (entry.Key == "includeSitemaps" && entry.Value is JsonValue value && value.TryGetValue<bool>(out var flag)) { include = flag; }
            else { errors[entry.Key] = ["Only a boolean includeSitemaps value is writable."]; }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<SitemapsRobotsSettings>();
        var changed = include.HasValue && (!site.Has<SitemapsRobotsSettings>() || include.Value != settings.IncludeSitemaps);
        if (changed)
        {
            settings.IncludeSitemaps = include.Value;
            site.Put(nameof(SitemapsRobotsSettings), settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
        return new() { Section = Describe(settings), Changed = changed };
    }
    private static SiteSettingsSectionResponse Describe(SitemapsRobotsSettings settings) => new()
    {
        Name = "sitemaps-robots", Source = "tenant", Values = new JsonObject { ["includeSitemaps"] = settings.IncludeSitemaps },
    };
}
