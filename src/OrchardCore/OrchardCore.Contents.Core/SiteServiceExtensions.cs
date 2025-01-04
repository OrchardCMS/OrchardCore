using OrchardCore.ContentManagement;
using OrchardCore.Entities;

namespace OrchardCore.Settings;

public static class SiteServiceExtensions
{
    /// <summary>
    /// Gets an instance of the specified custom settings if it exists.
    /// </summary>
    /// <param name="siteService">The site service.</param>
    /// <param name="name">The entry name.</param>
    /// <returns>A <see cref="ContentItem" /> instance that matches the given name, if one exists.</returns>
    public static async Task<ContentItem> GetCustomSettingsAsync(this ISiteService siteService, string name)
        => (await siteService.GetSiteSettingsAsync()).As<ContentItem>(name);
}
