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
        var site = _siteService.GetSiteSettings();

        options.MaxPageSize = site.MaxPageSize;

        // A site without a paged count keeps the one configured in 'OrchardCore:Navigation:PagerOptions'.
        if (site.MaxPagedCount > 0)
        {
            options.MaxPagedCount = site.MaxPagedCount;
        }

        options.PageSize = site.PageSize;
        options.AllowPageSizeSelection = site.AllowPageSizeSelection;
        options.PageSizeOptions = site.PageSizeOptions;
    }
}
