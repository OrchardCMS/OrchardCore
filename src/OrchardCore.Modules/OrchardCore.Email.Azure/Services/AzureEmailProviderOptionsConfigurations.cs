using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Azure.Services;

public sealed class AzureEmailProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly AzureEmailOptions _azureOptions;

    public AzureEmailProviderOptionsConfigurations(IOptions<AzureEmailOptions> azureOptions)
    {
        _azureOptions = azureOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(AzureEmailProvider))
        {
            IsEnabled = _azureOptions.IsEnabled,
        };

        options.TryAddProvider(AzureEmailProvider.TechnicalName, typeOptions);
    }
}
