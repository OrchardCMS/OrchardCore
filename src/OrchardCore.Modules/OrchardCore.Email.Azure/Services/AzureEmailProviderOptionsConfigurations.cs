using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ISiteService _siteService;

    public AzureEmailProviderOptionsConfigurations(
        IShellConfiguration shellConfiguration,
        ISiteService siteService)
    {
        _shellConfiguration = shellConfiguration;
        _siteService = siteService;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(AzureEmailProvider));

        var settings = _siteService.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<AzureEmailSettings>();

        var hasConnectionString = !string.IsNullOrEmpty(settings.ConnectionString);

        if (!hasConnectionString)
        {
            var connectionString = _shellConfiguration[$"OrchardCore_Email_Azure:{nameof(AzureEmailSettings.ConnectionString)}"];

            hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
        }

        var hasDefaultSender = !string.IsNullOrEmpty(settings.DefaultSender);

        if (!hasDefaultSender)
        {
            var defaultSender = _shellConfiguration[$"OrchardCore_Email_Azure:{nameof(AzureEmailSettings.DefaultSender)}"];

            hasDefaultSender = !string.IsNullOrWhiteSpace(defaultSender);
        }

        typeOptions.IsEnabled = hasConnectionString && hasDefaultSender;

        options.TryAddProvider(AzureEmailProvider.TechnicalName, typeOptions);
    }
}
