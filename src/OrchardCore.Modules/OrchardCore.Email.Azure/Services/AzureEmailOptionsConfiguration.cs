using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public sealed class AzureEmailOptionsConfiguration : IConfigureOptions<AzureEmailOptions>
{
    public const string ProtectorName = "AzureEmailProtector";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public AzureEmailOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
    }

    public void Configure(AzureEmailOptions options)
    {
        var settings = _siteService.GetSettingsAsync<AzureEmailSettings>()
            .GetAwaiter()
            .GetResult();

        options.IsEnabled = settings.IsEnabled;
        options.DefaultSender = settings.DefaultSender;

        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            options.ConnectionString = protector.Unprotect(settings.ConnectionString);
        }
    }
}
