using OrchardCore.Https.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Https.Services;

public class HttpsService : IHttpsService
{
    private readonly ISiteService _siteService;

    public HttpsService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public Task<HttpsSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<HttpsSettings>();
}
