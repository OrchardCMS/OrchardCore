using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services;

public class FacebookSettingsConfiguration : IConfigureOptions<FacebookSettings>
{
    private readonly IFacebookService _facebookService;

    public FacebookSettingsConfiguration(IFacebookService facebookService)
    {
        _facebookService = facebookService;
    }

    public void Configure(FacebookSettings options)
    {
        var settings = _facebookService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.AppId = settings.AppId;
        options.AppSecret = settings.AppSecret;
        options.Version = settings.Version;
        options.FBInit = settings.FBInit;
        options.FBInitParams = settings.FBInitParams;
        options.SdkJs = settings.SdkJs;
    }
}
