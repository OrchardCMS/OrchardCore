using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class AzureSmsOptionsConfiguration : IConfigureOptions<AzureSmsOptions>
{
    public const string ProtectorName = "AzureSmsProtector";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public AzureSmsOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
    }

    public void Configure(AzureSmsOptions options)
    {
        var settings = _siteService.GetSettingsAsync<AzureSmsSettings>()
            .GetAwaiter()
            .GetResult();

        options.IsEnabled = settings.IsEnabled;
        options.PhoneNumber = settings.PhoneNumber;

        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            options.ConnectionString = protector.Unprotect(settings.ConnectionString);
        }
    }
}
