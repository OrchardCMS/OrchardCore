using System.Threading.Tasks;

namespace OrchardCore.Settings;

public static class SiteServiceExtensions
{
    public static async Task<T> GetSettingsAsync<T>(this ISiteService siteService) where T : new()
        => (await siteService.GetSiteSettingsAsync()).As<T>();
}
