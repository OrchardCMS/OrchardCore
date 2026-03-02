using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Configuration;

public sealed class ReCaptchaSettingsConfiguration : IConfigureOptions<ReCaptchaSettings>
{
    private readonly ISiteService _site;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<ReCaptchaSettingsConfiguration> _logger;

    public const string ProtectorName = "ReCaptchaSettingsConfiguration";

    public ReCaptchaSettingsConfiguration(
        ISiteService site,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<ReCaptchaSettingsConfiguration> logger)
    {
        _site = site;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(ReCaptchaSettings options)
    {
        var settings = _site.GetSettings<ReCaptchaSettings>();

        var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

        if (!string.IsNullOrWhiteSpace(settings.SecretKey))
        {
            try
            {
                options.SecretKey = protector.Unprotect(settings.SecretKey);
            }
            catch
            {
                _logger.LogError("The secret key could not be decrypted. It may have been encrypted using a different key.");
            }
        }

        options.SiteKey = settings.SiteKey;
    }
}
