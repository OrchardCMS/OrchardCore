using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services;

/// <summary>
/// Applies the site <see cref="LocalizationSettings"/> to the <see cref="RequestLocalizationOptions"/>.
/// </summary>
/// <remarks>
/// This runs as a post configuration so that the site settings win over any other configuration, and the settings
/// are read on each options rebuild so that <see cref="IOptionsMonitor{TOptions}"/> observes the updated cultures.
/// </remarks>
internal sealed class LocalizationSettingsOptionsConfiguration : IPostConfigureOptions<RequestLocalizationOptions>
{
    private readonly ISiteService _siteService;
    private readonly IOptions<CultureOptions> _cultureOptions;

    public LocalizationSettingsOptionsConfiguration(
        ISiteService siteService,
        IOptions<CultureOptions> cultureOptions)
    {
        _siteService = siteService;
        _cultureOptions = cultureOptions;
    }

    public void PostConfigure(string name, RequestLocalizationOptions options)
    {
        if (name != Options.DefaultName)
        {
            return;
        }

        var settings = _siteService.GetSettings<LocalizationSettings>();
        var supportedCultures = LocalizationService.GetSupportedCultures(settings);

        // Set before the cultures are added as it is used to create them.
        options.CultureInfoUseUserOverride = !_cultureOptions.Value.IgnoreSystemSettings;
        options.FallBackToParentUICultures = options.FallBackToParentCultures = settings.FallBackToParentCulture;
        options
            .SetDefaultCulture(LocalizationService.GetDefaultCulture(settings))
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
    }
}
