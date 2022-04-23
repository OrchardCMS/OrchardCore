using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement.Core;

namespace OrchardCore.Settings;

public class ResourceSettingConfigureOptions : IConfigureOptions<ResourceSetting>
{
    private readonly ISiteService _siteService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResourceSettingConfigureOptions(ISiteService siteService, IHttpContextAccessor httpContextAccessor)
    {
        _siteService = siteService;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Configure(ResourceSetting options)
    {
        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

        options.ResourceDebugMode = site.ResourceDebugMode;
        options.UseCdn = site.UseCdn;
        options.CdnBaseUrl = site.CdnBaseUrl;
        options.AppendVersion = site.AppendVersion;
        options.ContentBasePath = _httpContextAccessor.HttpContext.Request.PathBase.Value;

    }

}
