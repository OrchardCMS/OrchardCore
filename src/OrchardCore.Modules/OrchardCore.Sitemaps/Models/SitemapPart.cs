using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Models;

/// <summary>
/// SitemapPart is an optional part, which allows individual content items to be excluded from sitemaps.
/// </summary>
public class SitemapPart : ContentPart
{
    [Description("Whether this content item uses its own sitemap settings.")]
    public bool OverrideSitemapConfig { get; set; }

    [Description("How frequently the content item is expected to change.")]
    public ChangeFrequency ChangeFrequency { get; set; } = ChangeFrequency.Daily;

    [Description("The sitemap priority from 0 through 10, representing 0.0 through 1.0.")]
    public int Priority { get; set; } = 5;

    [Description("Whether the content item is excluded from sitemaps.")]
    public bool Exclude { get; set; }
}
