using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public sealed class MicrosoftAccountSettingsConfiguration : IConfigureOptions<MicrosoftAccountSettings>
{
    private readonly IMicrosoftAccountService _microsoftAccountService;

    public MicrosoftAccountSettingsConfiguration(IMicrosoftAccountService microsoftAccountService)
    {
        _microsoftAccountService = microsoftAccountService;
    }

    public void Configure(MicrosoftAccountSettings options)
    {
        var settings = GetMicrosoftAccountSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings != null)
        {
            options.AppId = settings.AppId;
            options.AppSecret = settings.AppSecret;
            options.CallbackPath = settings.CallbackPath;
            options.SaveTokens = settings.SaveTokens;
        }
    }

    private async Task<MicrosoftAccountSettings> GetMicrosoftAccountSettingsAsync()
    {
        var settings = await _microsoftAccountService.GetSettingsAsync().ConfigureAwait(false);
        if (_microsoftAccountService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            return null;
        }

        return settings;
    }
}
