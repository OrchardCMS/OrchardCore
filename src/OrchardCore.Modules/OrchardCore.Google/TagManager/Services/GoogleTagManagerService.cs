using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager.Services;

public class GoogleTagManagerService : IGoogleTagManagerService
{
    private readonly ISiteService _siteService;

    public GoogleTagManagerService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public Task<GoogleTagManagerSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<GoogleTagManagerSettings>();
}
