using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Configurations;

public class TwilioOptionsConfiguration : IConfigureOptions<TwilioOptions>
{
    public const string ProtectorName = "TwilioSettingsConfiguration";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<TwilioOptionsConfiguration> _logger;

    public TwilioOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<TwilioOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(TwilioOptions options)
    {
        var settings = _siteService.GetSettings<TwilioSettings>();

        var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

        if (!string.IsNullOrWhiteSpace(settings.AuthToken))
        {
            try
            {
                options.AuthToken = protector.Unprotect(settings.AuthToken);
            }
            catch
            {
                _logger.LogError("The auth token could not be decrypted. It may have been encrypted using a different key.");
            }
        }

        options.AccountSID = settings.AccountSID;
        options.PhoneNumber = settings.PhoneNumber;
    }
}
