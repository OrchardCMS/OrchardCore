using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Security.Services;

public abstract class OAuthSettingsService<TAuthenticationSettings> where TAuthenticationSettings : OAuthSettings, new()
{
    private readonly ISiteService _siteService;

    public OAuthSettingsService(
        ISiteService siteService,
        IStringLocalizer<OAuthSettingsService<TAuthenticationSettings>> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    protected IStringLocalizer S { init; get; }

    public async Task<TAuthenticationSettings> GetSettingsAsync()
    {
        var settings = await _siteService.GetSiteSettingsAsync();

        return settings.As<TAuthenticationSettings>();
    }

    public async Task UpdateSettingsAsync(TAuthenticationSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var oldSettings = await _siteService.LoadSiteSettingsAsync();

        oldSettings.Properties[nameof(TAuthenticationSettings)] = JObject.FromObject(settings);

        await _siteService.UpdateSiteSettingsAsync(oldSettings);
    }

    public abstract IEnumerable<ValidationResult> ValidateSettings(TAuthenticationSettings settings);
}
