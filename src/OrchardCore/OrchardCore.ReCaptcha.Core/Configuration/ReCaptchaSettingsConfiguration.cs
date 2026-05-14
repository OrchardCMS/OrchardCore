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
        var settings = _site.GetSettings<ReCaptchaSettings>();

        if (settings != null)
        {
            // Only apply if NOT already set by appsettings.json via PostConfigure
            options.SiteKey ??= settings.SiteKey;
            options.SecretKey ??= settings.SecretKey;
            options.ReCaptchaScriptUri ??= settings.ReCaptchaScriptUri;
            options.ReCaptchaApiUri ??= settings.ReCaptchaApiUri;
        }
    }
}
