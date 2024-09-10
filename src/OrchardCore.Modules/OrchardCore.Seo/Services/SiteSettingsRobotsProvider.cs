using System.Text;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Services;

public class SiteSettingsRobotsProvider : IRobotsProvider
{
    private readonly ISiteService _siteService;
    private readonly AdminOptions _adminOptions;

    public SiteSettingsRobotsProvider(
        ISiteService siteService,
        IOptions<AdminOptions> adminOptions)
    {
        _siteService = siteService;
        _adminOptions = adminOptions.Value;
    }

    public async Task<string> GetContentAsync()
    {
        var settings = await _siteService.GetSettingsAsync<RobotsSettings>();

        var content = new StringBuilder();

        if (settings.AllowAllAgents)
        {
            content.AppendLine("User-agent: *");
        }

        if (settings.DisallowAdmin)
        {
            content.AppendLine($"Disallow: /{_adminOptions.AdminUrlPrefix}");
        }

        if (!string.IsNullOrEmpty(settings.AdditionalRules))
        {
            content.AppendLine(settings.AdditionalRules.Trim());
        }

        return content.ToString();
    }
}
