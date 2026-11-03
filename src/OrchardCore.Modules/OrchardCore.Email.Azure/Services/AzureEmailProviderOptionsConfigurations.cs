using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Azure.Services;

public sealed class AzureEmailProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly IOptionsMonitor<AzureEmailOptions> _azureOptions;
    private readonly IOptionsMonitor<DefaultAzureEmailOptions> _defaultAzureOptions;

    public AzureEmailProviderOptionsConfigurations(
        IOptionsMonitor<AzureEmailOptions> azureOptions,
        IOptionsMonitor<DefaultAzureEmailOptions> defaultAzureOptions)
    {
        _azureOptions = azureOptions;
        _defaultAzureOptions = defaultAzureOptions;
    }

    public void Configure(EmailProviderOptions options)
    {
        ConfigureTenantProvider(options);

        if (_defaultAzureOptions.CurrentValue.IsEnabled)
        {
            // Only configure the default provider, if settings are provided by the configuration provider.
            ConfigureDefaultProvider(options);
        }
    }

    private void ConfigureTenantProvider(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(AzureEmailProvider))
        {
            IsEnabled = _azureOptions.CurrentValue.IsEnabled,
        };

        options.TryAddProvider(AzureEmailProvider.TechnicalName, typeOptions);
    }

    private static void ConfigureDefaultProvider(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(DefaultAzureEmailProvider))
        {
            IsEnabled = true,
        };

        options.TryAddProvider(DefaultAzureEmailProvider.TechnicalName, typeOptions);
    }
}
