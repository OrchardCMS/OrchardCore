using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ILogger _logger;

    public AzureEmailProviderOptionsConfigurations(
        IShellConfiguration shellConfiguration,
        ILogger<AzureEmailProviderOptionsConfigurations> logger)
    {
        _shellConfiguration = shellConfiguration;
        _logger = logger;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(AzureEmailProvider));

        var connectionString = _shellConfiguration[$"OrchardCore_Email_Azure:{nameof(AzureEmailSettings.ConnectionString)}"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            typeOptions.IsEnabled = false;

            _logger.LogError("Azure Email is enabled but not active because the 'ConnectionString' is missing or empty in application configuration.");
        }
        else
        {
            typeOptions.IsEnabled = true;
        }

        options.TryAddProvider(AzureEmailProvider.TechnicalName, typeOptions);
    }
}
