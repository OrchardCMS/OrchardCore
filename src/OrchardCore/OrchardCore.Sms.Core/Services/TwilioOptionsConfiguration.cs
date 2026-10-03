using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public sealed class TwilioOptionsConfiguration : IConfigureOptions<TwilioOptions>
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISecretManager _secretManager;

    public TwilioOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
    }

    public void Configure(TwilioOptions options)
    {
        var settings = _siteService.GetSettings<TwilioSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.PhoneNumber = settings.PhoneNumber;
        options.AccountSID = settings.AccountSID;

        if (!string.IsNullOrWhiteSpace(settings.AuthTokenSecretName))
        {
            var secret = _secretManager.GetSecretAsync<TextSecret>(settings.AuthTokenSecretName).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException($"Twilio auth token secret '{settings.AuthTokenSecretName}' was not found.");
            options.AuthToken = secret.Text;
        }
#pragma warning disable CS0618 // Type or member is obsolete
        else if (!string.IsNullOrEmpty(settings.AuthToken))
        {
            var protector = _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);

            options.AuthToken = protector.Unprotect(settings.AuthToken);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
