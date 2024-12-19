using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Login.Services;

public class FacebookLoginService : IFacebookLoginService
{
    private readonly ISiteService _siteService;

    public FacebookLoginService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public Task<FacebookLoginSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<FacebookLoginSettings>();

    public async Task<FacebookLoginSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();
        return container.As<FacebookLoginSettings>();
    }

    public async Task UpdateSettingsAsync(FacebookLoginSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Properties[nameof(FacebookLoginSettings)] = JObject.FromObject(settings, JOptions.Default);
        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookLoginSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var results = ImmutableArray.CreateBuilder<ValidationResult>();
        return Task.FromResult<IEnumerable<ValidationResult>>(results);
    }
}
