using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Services;

public class ReverseProxyService
{
    private readonly ISiteService _siteService;

    public ReverseProxyService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public Task<ReverseProxySettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<ReverseProxySettings>();
}
