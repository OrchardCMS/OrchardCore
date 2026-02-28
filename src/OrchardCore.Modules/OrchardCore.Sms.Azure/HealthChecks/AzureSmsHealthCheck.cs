using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.HealthChecks;

public class AzureSmsHealthCheck : IHealthCheck
{
    private readonly AzureSmsOptions _azureSmsOptions;

    public AzureSmsHealthCheck(AzureSmsOptions azureSmsOptions) => _azureSmsOptions = azureSmsOptions;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await PingAzureSmsService(_azureSmsOptions.ConnectionString))
            {
                return HealthCheckResult.Healthy();
            }
            else
            {
                return HealthCheckResult.Unhealthy(description: $"Unable to connect to Azure SMS service.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Retrieving the status of the Azure SMS service failed.", ex);
        }
    }

    private static async Task<bool> PingAzureSmsService(string connectionString)
    {
        try
        {
            var endpoint = GetEndpointFromConnectionString(connectionString);
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new InvalidOperationException("Invalid connection string format.");
            }

            using var httpClient = new HttpClient();

            var payload = """
                {
                    "from": "+15551112222",
                    "smsRecipients": [
                        { "to": "+15555550100" }
                    ],
                    "message": "This is a test message — no real SMS will be delivered",
                    "smsSendOptions": { "enableDeliveryReport": true }
                }
                """;

            var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/sms?api-version=2021-03-07")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };

            var response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string GetEndpointFromConnectionString(string connectionString)
    {
        foreach (var part in connectionString.Split(';'))
        {
            if (part.StartsWith("endpoint=", StringComparison.OrdinalIgnoreCase))
            {
                return part.Substring("endpoint=".Length).TrimEnd('/');
            }
        }

        return null;
    }
}
