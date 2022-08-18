using Microsoft.Extensions.Options;
using OrchardCore.Navigation;

namespace OrchardCore.Settings;

public class PagerOptionsConfiguration : IPostConfigureOptions<PagerOptions>
{
    private readonly ISiteService _siteService;

    public PagerOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void PostConfigure(string name, PagerOptions options)
    {
        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

        options.MaxPageSize = site.MaxPageSize;
        options.MaxPagedCount = site.MaxPagedCount;
        options.PageSize = site.PageSize;
    }
}
