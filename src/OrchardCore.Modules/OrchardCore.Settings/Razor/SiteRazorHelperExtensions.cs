using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Settings;

public static class SiteRazorHelperExtensions
{
    /// <summary>
    /// Gets the site name.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    public static async Task<string> GetSiteName(this IOrchardHelper orchardHelper)
    {
        var siteService = orchardHelper.HttpContext.RequestServices.GetService<ISiteService>();

        var siteSettings = await siteService.GetSiteSettingsAsync();

        return siteSettings.SiteName;
    }
}
