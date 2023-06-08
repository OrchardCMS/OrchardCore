using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Services;

public class SiteSettingsRobotsProvider : IRobotsProvider
{
    private readonly ISiteService _siteService;

    public SiteSettingsRobotsProvider(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task<string> ContentAsync()
    {
        var settings = (await _siteService.GetSiteSettingsAsync()).As<RobotsSettings>();

        return settings.FileContent;
    }
}
