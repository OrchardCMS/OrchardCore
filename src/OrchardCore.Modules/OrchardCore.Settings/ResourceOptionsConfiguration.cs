using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Settings;

public class ResourceOptionsConfiguration : IPostConfigureOptions<ResourceOptions>
{
    private readonly ISiteService _siteService;

    public ResourceOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void PostConfigure(string name, ResourceOptions options)
    {
        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

        options.ResourceDebugMode = site.ResourceDebugMode;
        options.UseCdn = site.UseCdn;
        options.CdnBaseUrl = site.CdnBaseUrl;
        options.AppendVersion = site.AppendVersion;
    }
}
