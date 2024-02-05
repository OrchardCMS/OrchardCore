using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public class AzureEmailOptionsConfiguration : IConfigureOptions<AzureEmailOptions>
{
    public const string ProtectorName = "AzureEmailProtector";
    public const string SectionName = "OrchardCore_Email_Azure";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellConfiguration _shellConfiguration;

    public AzureEmailOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        IShellConfiguration shellConfiguration)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(AzureEmailOptions options)
    {
        var section = _shellConfiguration.GetSection(SectionName);

        // First we bind the options for the appsettings file, then we override the options from site settings.
        section.Bind(options);

        var settings = _siteService.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<AzureEmailSettings>();

        if (!string.IsNullOrWhiteSpace(settings.DefaultSender))
        {
            options.DefaultSender = settings.DefaultSender;
        }

        if (!options.PreventUIConnectionChange && !string.IsNullOrEmpty(settings.ConnectionString))
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            options.ConnectionString = protector.Unprotect(settings.ConnectionString);
        }
    }
}
