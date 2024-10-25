using System.ComponentModel;

namespace OrchardCore.Sitemaps.Models;

public class SitemapsRobotsSettings
{
    [DefaultValue(true)]
    public bool IncludeSitemaps { get; set; } = true;
}
