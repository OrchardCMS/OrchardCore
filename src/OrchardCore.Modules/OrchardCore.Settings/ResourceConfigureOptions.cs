using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Settings;

public class ResourceConfigureOptions : IPostConfigureOptions<ResourceSettings>
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

    public void PostConfigure(string name, ResourceSettings settings)
    {
        settings.Source = OptionSource.Database;

        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
        settings.Options.ResourceDebugMode = site.ResourceDebugMode;
        settings.Options.UseCdn = site.UseCdn;
        settings.Options.CdnBaseUrl = site.CdnBaseUrl;
        settings.Options.AppendVersion = site.AppendVersion;
        settings.Options.ContentBasePath = _httpContextAccessor.HttpContext.Request.PathBase.Value;
    }
}
