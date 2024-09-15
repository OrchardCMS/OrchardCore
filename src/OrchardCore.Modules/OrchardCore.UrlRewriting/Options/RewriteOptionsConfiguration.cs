using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Settings;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Rules;

namespace OrchardCore.UrlRewriting.Options;

public sealed class RewriteOptionsConfiguration : IConfigureOptions<RewriteOptions>
{
    private readonly ISiteService _siteService;

    private readonly AdminOptions _adminOptions;

    public RewriteOptionsConfiguration(ISiteService siteService, IOptions<AdminOptions> adminOptions)
    {
        _siteService = siteService;
        _adminOptions = adminOptions.Value;
    }

    public void Configure(RewriteOptions options)
    {
        var settings = _siteService.GetSettingsAsync<UrlRewritingSettings>()
            .GetAwaiter()
            .GetResult();

        using var apacheModRewrite = new StringReader(settings.ApacheModRewrite ?? string.Empty);

        options.AddApacheModRewrite(apacheModRewrite);

        if (options.Rules.Count > 0)
        {
            // Exclude admin ui requests to prevent accidental access bricking by provided rules
            options.Rules.Insert(0, new ExcludeAdminUIRule(_adminOptions.AdminUrlPrefix));
        }
    }
}
