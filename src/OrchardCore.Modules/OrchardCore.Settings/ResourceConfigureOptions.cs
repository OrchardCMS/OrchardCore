using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Settings;

public class ResourceConfigureOptions : IPostConfigureOptions<ResourceOptions>
{
    private readonly ISiteService _siteService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResourceConfigureOptions(
        ISiteService siteService,
        IHttpContextAccessor httpContextAccessor)
    {
        _siteService = siteService;
        _httpContextAccessor = httpContextAccessor;
    }

    public void PostConfigure(string name, ResourceOptions options)
    {
        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

        options.ResourceDebugMode = site.ResourceDebugMode;
        options.UseCdn = site.UseCdn;
        options.CdnBaseUrl = site.CdnBaseUrl;
        options.AppendVersion = site.AppendVersion;
        options.ContentBasePath = _httpContextAccessor.HttpContext.Request.PathBase.Value;
    }
}
