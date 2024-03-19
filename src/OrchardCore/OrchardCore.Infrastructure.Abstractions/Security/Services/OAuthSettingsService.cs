using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Settings;

namespace OrchardCore.Security.Services;

public abstract class OAuthSettingsService<TAuthenticationSettings>
    where TAuthenticationSettings : OAuthSettings, new()
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

    public async Task<TAuthenticationSettings> LoadSettingsAsync()
    {
        var settings = await _siteService.LoadSiteSettingsAsync();

        return settings.As<TAuthenticationSettings>();
    }

    public async Task UpdateSettingsAsync(TAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var siteSettings = await _siteService.LoadSiteSettingsAsync();

        siteSettings.Properties[typeof(TAuthenticationSettings).Name] = JObject.FromObject(settings);

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }

    public abstract IEnumerable<ValidationResult> ValidateSettings(TAuthenticationSettings settings);
}
