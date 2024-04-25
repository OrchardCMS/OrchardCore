using OrchardCore.Entities;

namespace OrchardCore.Settings.Core;

public static class SiteExtensions
{
    public static ISite QueueSiteReload(this ISite site)
    {
        site.Put(new SiteReload()
        {
            Reload = true,
        });

        return site;
    }

    /// <summary>
    /// Removes the Reload property from the site.
    /// </summary>
    /// <param name="site"></param>
    /// <returns>true is the site should be reloaded otherwise false.</returns>
    public static bool DequeueSiteReload(this ISite site)
    {
        // Get the current status before removing it.
        var reload = site.As<SiteReload>()?.Reload ?? false;

        site.Properties.Remove(nameof(SiteReload));

        return reload;
    }

    private sealed class SiteReload
    {
        public bool Reload { get; set; }
    }
}
