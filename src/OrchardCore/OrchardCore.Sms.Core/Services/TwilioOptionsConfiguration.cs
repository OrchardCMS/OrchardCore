using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public sealed class TwilioOptionsConfiguration : IConfigureOptions<TwilioOptions>
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public TwilioOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
    }

    public void Configure(TwilioOptions options)
    {
        var settings = _siteService.GetSettings<TwilioSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.PhoneNumber = settings.PhoneNumber;
        options.AccountSID = settings.AccountSID;

        if (!string.IsNullOrEmpty(settings.AuthToken))
        {
            var protector = _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);

            options.AuthToken = protector.Unprotect(settings.AuthToken);
        }
    }
}
