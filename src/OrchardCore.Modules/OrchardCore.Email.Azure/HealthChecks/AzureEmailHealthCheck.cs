using System.Data.Common;
using Azure.Communication.Email;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.HealthChecks;

internal sealed class AzureEmailHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AzureEmailOptions _azureEmailOptions;

    public AzureEmailHealthCheck(IHttpClientFactory httpClientFactory, AzureEmailOptions azureEmailOptions)
    {
        _httpClientFactory = httpClientFactory;
        _azureEmailOptions = azureEmailOptions;
    }

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
                return HealthCheckResult.Unhealthy(description: "Unable to connect to Azure Email service.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Retrieving the status of the Azure Email service failed.", ex);
        }
    }

    private async Task<bool> PingAzureEmailService(string connectionString)
    {
        try
        {
            var emailClient = new EmailClient(connectionString);

            var endpoint = GetEndpointFromConnectionString(connectionString);
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new InvalidOperationException("Invalid connection string format.");
            }

            var httpClient = _httpClientFactory.CreateClient();
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
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString,
        };

        builder.TryGetValue("endpoint", out var endpoint);

        return endpoint.ToString();
    }
}
