using Azure.Communication.Sms;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.HealthChecks;

internal sealed class AzureSmsHealthCheck : IHealthCheck
{
    private readonly AzureSmsOptions _azureSmsOptions;

    public AzureSmsHealthCheck(IOptions<AzureSmsOptions> azureSmsOptions) => _azureSmsOptions = azureSmsOptions.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var smsClient = new SmsClient(_azureSmsOptions.ConnectionString);

            await smsClient.SendAsync(_azureSmsOptions.PhoneNumber, "+00000", "Test", cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy(description: $"Unable to connect to Azure SMS service.");
        }
    }
}
