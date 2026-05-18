using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.HealthChecks;

internal sealed class AzureEmailHealthCheck : IHealthCheck
{
    private readonly AzureEmailOptions _azureEmailOptions;

    public AzureEmailHealthCheck(IOptions<AzureEmailOptions> azureEmailOptions)
    {
        _azureEmailOptions = azureEmailOptions.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailClient = new EmailClient(_azureEmailOptions.ConnectionString);

            await emailClient.SendAsync(
                WaitUntil.Completed,
                senderAddress: _azureEmailOptions.DefaultSender,
                recipientAddress: "invalid@domain.com",
                subject: "Test",
                htmlContent: "<p>Test</p>",
                cancellationToken: cancellationToken
            );

            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy(description: "Unable to connect to Azure Email service.");
        }
    }
}
