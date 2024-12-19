using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Core.Services;

namespace OrchardCore.Email.Azure.Services;

public sealed class AzureEmailProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly AzureEmailOptions _azureOptions;
    private readonly DefaultAzureEmailOptions _defaultAzureOptions;

    public AzureEmailProviderOptionsConfigurations(
        IOptions<AzureEmailOptions> azureOptions,
        IOptions<DefaultAzureEmailOptions> defaultAzureOptions)
    {
        _azureOptions = azureOptions.Value;
        _defaultAzureOptions = defaultAzureOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        ConfigureTenantProvider(options);

        if (_defaultAzureOptions.IsEnabled)
        {
            // Only configure the default provider, if settings are provided by the configuration provider.
            ConfigureDefaultProvider(options);
        }
    }

    private void ConfigureTenantProvider(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(AzureEmailProvider))
        {
            IsEnabled = _azureOptions.IsEnabled,
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
