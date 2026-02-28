using Azure.Communication.Email;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.HealthChecks;

public class AzureEmailHealthCheck : IHealthCheck
{
    private readonly AzureEmailOptions _azureEmailOptions;

    public AzureEmailHealthCheck(AzureEmailOptions azureEmailOptions) => _azureEmailOptions = azureEmailOptions;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await PingAzureEmailService(_azureEmailOptions.ConnectionString))
            {
                return HealthCheckResult.Healthy();
            }
            else
            {
                return HealthCheckResult.Unhealthy(description: $"Unable to connect to Azure Email service.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Retrieving the status of the Azure Email service failed.", ex);
        }
    }

    private static async Task<bool> PingAzureEmailService(string connectionString)
    {
        try
        {
            var emailClient = new EmailClient(connectionString);

            var endpoint = GetEndpointFromConnectionString(connectionString);
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new InvalidOperationException("Invalid connection string format.");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString("R"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "AzureEmailPingClient");

            // NOTE: This is a HEAD request to avoid sending data
            var response = await httpClient.GetAsync($"{endpoint}/emails?api-version=2023-03-31", HttpCompletionOption.ResponseHeadersRead);

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
