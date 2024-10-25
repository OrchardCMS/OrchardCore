using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Configuration;

public sealed class ReCaptchaSettingsConfiguration : IConfigureOptions<ReCaptchaSettings>
{
    private readonly ISiteService _site;

    public ReCaptchaSettingsConfiguration(ISiteService site)
    {
        _site = site;
    }

    public void Configure(ReCaptchaSettings options)
    {
        var settings = _site.GetSettingsAsync<ReCaptchaSettings>()
            .GetAwaiter()
            .GetResult();

        options.SiteKey = settings.SiteKey;
        options.SecretKey = settings.SecretKey;
    }
}
