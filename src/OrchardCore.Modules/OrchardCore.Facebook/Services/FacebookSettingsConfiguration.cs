using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services;

public sealed class FacebookSettingsConfiguration : IConfigureOptions<FacebookSettings>
{
    private readonly IFacebookService _facebookService;

    public FacebookSettingsConfiguration(IFacebookService facebookService)
    {
        _facebookService = facebookService;
    }

    public void Configure(FacebookSettings options)
    {
        var settings = GetFacebookSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings != null)
        {
            options.AppId = settings.AppId;
            options.AppSecret = settings.AppSecret;
            options.Version = settings.Version;
            options.FBInit = settings.FBInit;
            options.FBInitParams = settings.FBInitParams;
            options.SdkJs = settings.SdkJs;
        }
    }

    private async Task<FacebookSettings> GetFacebookSettingsAsync()
    {
        var settings = await _facebookService.GetSettingsAsync();

        if (_facebookService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            return null;
        }

        return settings;
    }
}
