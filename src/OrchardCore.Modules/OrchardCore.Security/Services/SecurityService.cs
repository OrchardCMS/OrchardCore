using OrchardCore.Security.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Security.Services;

public class SecurityService : ISecurityService
{
    private readonly ISiteService _siteService;

    public SecurityService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public Task<SecuritySettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<SecuritySettings>();
}
