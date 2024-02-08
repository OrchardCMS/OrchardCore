using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Services;
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

        var azureEmailOptions = _shellConfiguration.GetSection(AzureEmailOptionsConfiguration.SectionName).Get<AzureEmailOptions>();

        var hasConnectionString = !string.IsNullOrEmpty(settings.ConnectionString);

        if (!hasConnectionString)
        {
            hasConnectionString = !string.IsNullOrWhiteSpace(azureEmailOptions?.ConnectionString);
        }

        var hasDefaultSender = !string.IsNullOrEmpty(settings.DefaultSender);

        if (!hasDefaultSender)
        {
            hasDefaultSender = !string.IsNullOrWhiteSpace(azureEmailOptions?.DefaultSender);
        }

        typeOptions.IsEnabled = settings.IsEnabled ?? (hasConnectionString && hasDefaultSender);

        options.TryAddProvider(AzureEmailProvider.TechnicalName, typeOptions);
    }
}
