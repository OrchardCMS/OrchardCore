using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ResourceManagement.Abstractions;

namespace OrchardCore.Settings;

public class ResourceSettingProvider : IResourceSettingProvider
{
    private readonly ISiteService _siteService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResourceSettingProvider(ISiteService siteService, IHttpContextAccessor httpContextAccessor)
    {
        _siteService = siteService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResourceSetting> GetAsync()
    {
        ISite site = await _siteService.GetSiteSettingsAsync();

        return new ResourceSetting()
        {
            ResourceDebugMode = site.ResourceDebugMode,
            UseCdn = site.UseCdn,
            CdnBaseUrl = site.CdnBaseUrl,
            AppendVersion = site.AppendVersion,
            ContentBasePath = _httpContextAccessor.HttpContext.Request.PathBase.Value,
        };
    }
}
