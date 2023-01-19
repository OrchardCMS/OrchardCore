using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public class MicrosoftAccountSettingsConfiguration : IConfigureOptions<MicrosoftAccountSettings>
{
    private readonly IMicrosoftAccountService _microsoftAccountService;

    public MicrosoftAccountSettingsConfiguration(IMicrosoftAccountService microsoftAccountService)
    {
        _microsoftAccountService = microsoftAccountService;
    }

    public void Configure(MicrosoftAccountSettings options)
    {
        var settings = _microsoftAccountService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.AppId = settings.AppId;
        options.AppSecret= settings.AppSecret;
        options.CallbackPath = settings.CallbackPath;
        options.SaveTokens = settings.SaveTokens;
    }
}
