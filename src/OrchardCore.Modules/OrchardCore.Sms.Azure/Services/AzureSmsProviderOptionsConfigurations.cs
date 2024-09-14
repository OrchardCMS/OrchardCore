using Microsoft.Extensions.Options;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class AzureSmsProviderOptionsConfigurations : IConfigureOptions<SmsProviderOptions>
{
    private readonly AzureSmsOptions _azureOptions;
    private readonly DefaultAzureSmsOptions _defaultAzureOptions;

    public AzureSmsProviderOptionsConfigurations(
        IOptions<AzureSmsOptions> azureOptions,
        IOptions<DefaultAzureSmsOptions> defaultAzureOptions)
    {
        _azureOptions = azureOptions.Value;
        _defaultAzureOptions = defaultAzureOptions.Value;
    }

    public void Configure(SmsProviderOptions options)
    {
        ConfigureTenantProvider(options);

        if (_defaultAzureOptions.IsEnabled)
        {
            // Configure the default provider only if settings are supplied by the configuration provider.
            ConfigureDefaultProvider(options);
        }
    }

    private void ConfigureTenantProvider(SmsProviderOptions options)
    {
        var typeOptions = new SmsProviderTypeOptions(typeof(AzureSmsProvider))
        {
            IsEnabled = _azureOptions.IsEnabled,
        };

        options.TryAddProvider(AzureSmsProvider.TechnicalName, typeOptions);
    }

    private static void ConfigureDefaultProvider(SmsProviderOptions options)
    {
        var typeOptions = new SmsProviderTypeOptions(typeof(DefaultAzureSmsProvider))
        {
            IsEnabled = true,
        };

        options.TryAddProvider(DefaultAzureSmsProvider.TechnicalName, typeOptions);
    }
}
